import click
import datetime
import pydantic
import textwrap
import yaml
from typing import cast

from alvtime_cli import config, model
from alvtime_cli.commands.pull import pull as pull_command
from alvtime_cli.local_service import LocalService
from alvtime_cli.param_types import DateParam


class EditEntry(pydantic.BaseModel):
    task_id: int | None = None
    task: str
    date: datetime.date | None = None
    start: datetime.time
    stop: datetime.time
    comment: str | None = None
    ref: int | None = None

    @pydantic.field_validator("start", "stop", mode="before")
    def parse_time_string(cls, v):
        if isinstance(v, str):
            return datetime.time.fromisoformat(v)
        return v

    @pydantic.field_serializer("start", "stop")
    def serialize_time(self, v: datetime.time):
        return v.strftime("%H:%M")


class EditBreak(pydantic.BaseModel):
    date: datetime.date | None = None
    start: datetime.time
    stop: datetime.time
    comment: str | None = None
    ref: int | None = None

    @pydantic.field_validator("start", "stop", mode="before")
    def parse_time_string(cls, v):
        if isinstance(v, str):
            return datetime.time.fromisoformat(v)
        return v

    @pydantic.field_serializer("start", "stop")
    def serialize_time(self, v: datetime.time):
        return v.strftime("%H:%M")


class EditModel(pydantic.BaseModel):
    entries: list[EditEntry] = []
    breaks: list[EditBreak] = []

    model_config = {"extra": "forbid"}


def _editable_entry(entry: model.TimeEntry, aliases: list[model.TaskAlias]) -> EditEntry:
    alias = next((a for a in aliases if a.task.id == entry.task_id), None)
    if alias:
        task_name = alias.name
    else:
        task_name = _format_task_string(entry.task)
    return EditEntry(
            ref=entry.id,
            task=task_name,
            start=_round_time_to_minute(entry.start.time()),
            stop=_round_time_to_minute((entry.start + entry.duration).time()),
            comment=entry.comment)


def _editable_break(break_: model.TimeBreak) -> EditBreak:
    return EditBreak(
            ref=break_.id,
            start=_round_time_to_minute(break_.start.time()),
            stop=_round_time_to_minute((break_.start + break_.duration).time()),
            comment=break_.comment)


def _drop_empty(obj):
    """Recursively remove empty lists, dicts, and None values."""
    if isinstance(obj, dict):
        return {
            k: _drop_empty(v)
            for k, v in obj.items()
            if v not in (None, [], {}) and _drop_empty(v) is not None
        }
    elif isinstance(obj, list):
        return [_drop_empty(v) for v in obj if v not in (None, [], {})]
    else:
        return obj


def _round_time_to_minute(t: datetime.time) -> datetime.time:
    # Combine with a dummy date to allow arithmetic
    dt = datetime.datetime.combine(datetime.datetime.min, t)

    # Round to nearest minute
    if dt.second >= 30:
        dt += datetime.timedelta(minutes=1)
    dt = dt.replace(second=0, microsecond=0)

    return dt.time()


@click.command(help="Edit entries for a given date")
@click.argument("date", type=DateParam, default=datetime.date.today(), required=False)
@click.pass_context
def edit(ctx, date):
    if not config.get(config.Keys.auto_sync):
        raise click.ClickException("Not possible to edit when auto-sync is disabled")

    # Pull entries for the given date
    click.echo("Pulling...")
    ctx.invoke(pull_command,
               from_=date,
               to=date)
    click.echo()

    service: LocalService = cast(LocalService, ctx.obj)

    aliases = service.get_aliases()

    original = EditModel(
            entries=map(lambda x: _editable_entry(x, aliases),
                        service.get_entries(date, date)),
            breaks=map(_editable_break, service.get_breaks(date, date)))

    instructions = textwrap.dedent("""
        # Add, modify or delete entries in this file. Please note:
        # - Do NOT change the 'ref' field
        # - If adding entries, do not provide a 'ref' field
        #
    """).lstrip()

    text = instructions + yaml.dump(
            _drop_empty(original.model_dump(by_alias=True)),
            sort_keys=False,
            width=float("inf"))

    # Let the user edit the YAML in their default editor
    try:
        text_response = click.edit(text, extension=".yaml")
    except click.exceptions.ClickException:
        text_response = None

    # If not cancelled or not saved
    if text_response is None:
        click.echo("Cancelled.", err=True)
        return

    # Parse the YAML
    try:
        untyped_response = yaml.safe_load(text_response)
    except yaml.YAMLError as e:
        click.echo(f"YAML error: {e}", err=True)
        return

    # Treat empty file as empty
    if untyped_response is None:
        untyped_response = {"entries": [], "breaks": []}

    # Deserialize the EditModel
    try:
        response = EditModel.model_validate(untyped_response)
    except pydantic.ValidationError as e:
        messages = ["Validation failed"]
        for err in e.errors():
            loc = ".".join(str(p) for p in err["loc"])
            msg = err["msg"]
            messages.append(f"  - {loc}: {msg}")
        raise click.exceptions.ClickException("\n".join(messages))

    # Hydrate date
    for entry in original.entries + original.breaks + response.entries + response.breaks:
        entry.date = date

    # Hydrate task IDs
    all_tasks = service.get_all_tasks(include_locked=True)
    for entry in original.entries + response.entries:
        entry.task_id = _task_id_from_task_string(entry.task, all_tasks, aliases)

    _perform_changes(service, original, response)

    # Push the entries to cloud
    click.echo("Pushing...")
    result = service.push(date, date)

    if result.pushed_time_entries > 0:
        click.echo(f"Pushed {result.pushed_time_entries} entries")


def _perform_changes(service: LocalService, original: EditModel, response: EditModel):
    # Check for changes in original entries
    for original_entry in original.entries:
        response_entry = next((e for e in response.entries if e.ref == original_entry.ref), None)

        # Is response entry equal to original?
        if original_entry == response_entry:
            continue

        # Is entry deleted in response?
        if response_entry is None:
            service.delete_time_entry(original_entry.ref)
            continue

        # Entry has changed
        service.update_time_entry(_to_time_entry(response_entry))

    # Check for new entries
    for response_entry in response.entries:
        # Skip entries with 'ref' as these are not new
        if response_entry.ref is not None:
            continue

        service.add_time_entry(_to_time_entry(response_entry))

    # Check for changes in original breaks
    for original_break in original.breaks:
        response_break = next((e for e in response.breaks if e.ref == original_break.ref), None)

        # Is response break equal to original?
        if original_break == response_break:
            continue

        # Is break deleted in response?
        if response_break is None:
            service.delete_break(original_break.ref)
            continue

        # Break has changed
        service.update_break(_to_time_break(response_break))

    # Check for new breaks
    for response_break in response.breaks:
        # Skip breaks with 'ref' as these are not new
        if response_break.ref is not None:
            continue

        service.add_break(_to_time_break(response_break))


def _to_time_entry(edit_entry: EditEntry):
    start = datetime.datetime.combine(edit_entry.date, edit_entry.start).astimezone()
    stop = datetime.datetime.combine(edit_entry.date, edit_entry.stop).astimezone()

    return model.TimeEntry(
        id=edit_entry.ref,
        task_id=edit_entry.task_id,
        start=start,
        duration=(stop-start),
        is_open=False,
        comment=edit_entry.comment)


def _to_time_break(edit_break: EditBreak):
    start = datetime.datetime.combine(edit_break.date, edit_break.start).astimezone()
    stop = datetime.datetime.combine(edit_break.date, edit_break.stop).astimezone()

    return model.TimeBreak(
        id=edit_break.ref,
        start=start,
        duration=(stop-start),
        comment=edit_break.comment)


def _task_id_from_task_string(task_string: str, all_tasks: list[model.Task], aliases: list[model.TaskAlias]) -> int:
    task_id = next((a.task.id for a in aliases if a.name == task_string), None)
    if task_id is not None:
        return task_id

    task_id = next((t.id for t in all_tasks if _format_task_string(t) == task_string), None)
    if task_id is not None:
        return task_id

    raise AttributeError(f"Task '{task_string}' not found")


def _format_task_string(task: model.Task) -> str:
    return f"[{task.id}] {task.customer_name} {task.project_name} {task.name}"
