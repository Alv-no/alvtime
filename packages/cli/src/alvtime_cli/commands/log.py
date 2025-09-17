from datetime import date, datetime, timedelta
import enum
from typing import cast
import click

from alvtime_cli.utils import group_by
from alvtime_cli.param_types import DateParam
from alvtime_cli.local_service import LocalService
from alvtime_cli.utils import style_time_entry, style_task, iterate_dates
from alvtime_cli import model


def _set_duration_for_open_entry(entry: model.TimeEntry) -> model.TimeEntry:
    if not entry.duration:
        entry.duration = (datetime.now().replace(microsecond=0).astimezone() - entry.start)
    return entry


class DetailLevel(enum.Enum):
    summary = enum.auto()
    standard = enum.auto()
    full = enum.auto()


@click.command(help="Outputs activity log")
@click.option("--from", "from_", type=DateParam, default=date.today())
@click.option("--to", type=DateParam, default=date.today())
@click.option("--details", "-d",
              type=click.Choice(DetailLevel, case_sensitive=False),
              default=DetailLevel.standard,
              help="Print full details")
@click.pass_context
def log(ctx, from_: date, to: date, details: DetailLevel):
    service = cast(LocalService, ctx.obj)

    all_entries = map(_set_duration_for_open_entry,
                      service.get_entries(from_, to))
    entries_by_date = group_by(all_entries,
                               lambda e: e.start.date())
    for current_date in iterate_dates(from_, to):
        entries = entries_by_date.get(current_date, [])
        total = sum((e.duration for e in entries), timedelta())

        click.echo(current_date, nl=False)
        if total:
            click.echo(f" ({total})")
        else:
            click.echo("  ---")

        if details == DetailLevel.summary:
            continue

        entries_by_task = group_by(entries,
                                   lambda e: e.task_id)

        if details == DetailLevel.standard:
            for task_id in sorted(entries_by_task.keys()):
                task = entries_by_task[task_id][0].task
                task_total = sum((e.duration for e in entries_by_task[task_id]), timedelta())
                click.echo(f"    {task_total} - {style_task(task)}")

        elif details == DetailLevel.full:
            for entry in entries:
                task = entry.task
                task_total = entry.duration
                click.echo(f"    {task_total} - {style_time_entry(entry)}")

        click.echo()
