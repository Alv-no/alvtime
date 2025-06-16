from datetime import date
from typing import cast
import click

from alvtime_cli.param_types import DateParam
from alvtime_cli.local_service import LocalService, TaskAlreadyStartedError


@click.command(help="Synchronizes time entries with the server, pulling first, then pushing.")
@click.option("--from", "from_", type=DateParam, default=date.today())
@click.option("--to", type=DateParam, default=date.today())
@click.pass_context
def sync(ctx, from_: date, to: date):
    service = cast(LocalService, ctx.obj)

    try:
        result = service.pull(from_, to)

        if result.local_entries_created > 0:
            click.echo(f"Pulled {result.pulled_task_count} entries, created {result.local_entries_created}")

        result = service.push(from_, to)

        if result.pushed_time_entries > 0:
            click.echo(f"Pushed {result.pushed_time_entries} entries")
    except TaskAlreadyStartedError:
        raise click.exceptions.ClickException("Task is running, you must stop first")
