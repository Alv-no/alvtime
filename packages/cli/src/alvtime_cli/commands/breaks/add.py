import click
from datetime import datetime
from typing import cast

from alvtime_cli import config, model
from alvtime_cli.commands.sync import sync
from alvtime_cli.param_types import DateTimeParam
from alvtime_cli.utils import handle_exceptions, style
from alvtime_cli.local_service import LocalService


@click.command(help="Add a break")
@click.argument("start", type=DateTimeParam, required=True)
@click.argument("stop", type=DateTimeParam, required=True)
@click.argument("comment", type=str, default="", required=False)
@click.pass_context
@handle_exceptions()
def add(ctx: click.Context, start: datetime, stop: datetime, comment: str):
    service = cast(LocalService, ctx.obj)

    break_ = service.add_break(model.TimeBreak(
            start=start,
            duration=(stop-start),
            comment=comment))

    duration = break_.duration
    click.echo(f"Added break for {style(duration, 'time')}")

    if config.get(config.Keys.auto_sync):
        ctx.invoke(sync,
                   from_=break_.start.date(),
                   to=break_.start.date())
