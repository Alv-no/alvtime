from datetime import date
from typing import cast
import click

from alvtime_cli.param_types import DateParam
from alvtime_cli.local_service import LocalService, TaskAlreadyStartedError


@click.command(help="Pulls time entries from the server, merging it with local storage")
@click.option("--from", "from_", type=DateParam, default=date.today())
@click.option("--to", type=DateParam, default=date.today())
@click.pass_context
def pull(ctx, from_: date, to: date):
    service = cast(LocalService, ctx.obj)

    try:
        result = service.pull(from_, to)

        if result.local_entries_created == 0:
            click.echo("Already up to date")
        else:
            click.echo(f"Pulled {result.pulled_task_count} entries, created {result.local_entries_created}")
    except TaskAlreadyStartedError:
        raise click.exceptions.ClickException("Task is running, you must stop first")
