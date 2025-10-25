from datetime import date, datetime, timedelta
import enum
from typing import cast
import click

from alvtime_cli.utils import group_by
from alvtime_cli.param_types import DateParam
from alvtime_cli.local_service import LocalService
from alvtime_cli.utils import style, style_task, iterate_dates
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
              default=DetailLevel.full,
              help="Print full details")
@click.pass_context
def log(ctx, from_: date, to: date, details: DetailLevel):
    service: LocalService = cast(LocalService, ctx.obj)

    all_entries = map(_set_duration_for_open_entry,
                      service.get_entries(from_, to))
    breakified_entries = map(_set_duration_for_open_entry,
                             service.get_entries(from_, to, breakify=True))
    all_entries_by_date = group_by(all_entries,
                                   lambda e: e.start.date())
    breakified_entries_by_date = group_by(breakified_entries,
                                          lambda e: e.start.date())
    all_breaks_by_date = group_by(service.get_breaks(from_, to),
                                  lambda e: e.start.date())
    for current_date in iterate_dates(from_, to):
        all_entries = all_entries_by_date.get(current_date, [])
        breakified_entries = breakified_entries_by_date.get(current_date, [])
        breakified_total = sum((e.duration for e in breakified_entries), timedelta())
        breaks = all_breaks_by_date.get(current_date, [])
        breaks_total = sum((b.duration for b in breaks), timedelta())

        click.echo(current_date, nl=False)
        if breakified_total:
            click.echo(f" ({breakified_total})")
        else:
            click.echo("  ---")

        if details == DetailLevel.summary:
            continue

        all_entries_by_task = group_by(all_entries,
                                       lambda e: e.task_id)
        breakified_entries_by_task = group_by(breakified_entries,
                                              lambda e: e.task_id)

        for task_id in sorted(breakified_entries_by_task.keys()):
            task = breakified_entries_by_task[task_id][0].task
            task_total = sum((e.duration for e in breakified_entries_by_task[task_id]), timedelta())
            click.echo(f"  {task_total} - {style_task(task)}")

            if details == DetailLevel.standard:
                continue

            for entry in all_entries_by_task[task_id]:
                task = entry.task
                task_total = entry.duration
                if entry.duration:
                    stop = (entry.start + entry.duration).strftime("%H:%M")
                else:
                    stop = ""
                click.echo("    ", nl=False)
                click.echo(f"{style(entry.duration, 'duration')} ", nl=False)
                time_string = f"({entry.start.strftime('%H:%M')} - {stop}) "
                click.echo(style(time_string, "time"), nl=False)
                click.echo(f"{style(entry.comment, 'comment')}")

        if details == DetailLevel.full and breaks_total:
            click.echo(f" -{breaks_total} - {style('Breaks', 'comment')}")
            for break_ in breaks:
                if break_.duration:
                    stop = (break_.start + break_.duration).strftime("%H:%M")
                else:
                    stop = ""
                click.echo(f"    {style(break_.duration, 'duration')} ", nl=False)
                time_string = f"({break_.start.strftime('%H:%M')} - {stop}) "
                click.echo(style(time_string, "time"), nl=False)
                click.echo(f"{style(break_.comment, 'comment')}")

        click.echo()
