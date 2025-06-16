import click
from typing import cast
from datetime import datetime

from alvtime_cli.param_types import DateTimeParam
from alvtime_cli.local_service import LocalService, TaskAlreadyStartedError, NoLastEntryError
from alvtime_cli.utils import style_time_entry


@click.command(help="Restarts last activity")
@click.option("--at", type=DateTimeParam)
@click.argument("comment", required=False)
@click.pass_context
def restart(ctx, at: datetime = None, comment: str = None):
    service = cast(LocalService, ctx.obj)

    try:
        time_entry = service.restart(
            at=at,
            comment=comment)
    except TaskAlreadyStartedError:
        raise click.exceptions.ClickException("Project already running")
    except NoLastEntryError:
        raise click.exceptions.ClickException("No previous entry to restart")

    click.echo(style_time_entry(time_entry))
