import click
from datetime import datetime
from typing import cast
from alvtime_cli import config
from alvtime_cli.local_service import LocalService, TaskNotRunningError
from alvtime_cli.param_types import DateTimeParam
from alvtime_cli.utils import style_time_entry
from alvtime_cli.commands.sync import sync


@click.command(help="Stops current activity")
@click.option("--at", type=DateTimeParam)
@click.option("--comment", type=str)
@click.pass_context
def stop(ctx, at: datetime = None, comment: str = None):
    service = cast(LocalService, ctx.obj)

    try:
        time_entry = service.stop(
            at=at,
            comment=comment)
    except TaskNotRunningError:
        raise click.exceptions.ClickException("No project running")
    except ValueError as ex:
        raise click.exceptions.ClickException(str(ex))

    click.echo(style_time_entry(time_entry))

    if config.get(config.Keys.auto_sync):
        ctx.invoke(sync,
                   from_=time_entry.start.date(),
                   to=time_entry.start.date())
