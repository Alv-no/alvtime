from datetime import date, timedelta
import enum
from typing import cast
import click

from alvtime_cli.utils import group_by
from alvtime_cli.param_types import DateParam
from alvtime_cli.local_service import LocalService
from alvtime_cli.utils import style_time_entry, style_task


def _iterate_dates(start_date, end_date):
    current = start_date
    while current <= end_date:
        yield current
        current += timedelta(days=1)


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

    entries_by_date = group_by(service.get_entries(from_, to),
                               lambda e: e.start.date())
    for current_date in _iterate_dates(from_, to):
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
