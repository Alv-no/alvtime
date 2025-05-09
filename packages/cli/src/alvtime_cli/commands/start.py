from datetime import datetime
from typing import cast
import click

from alvtime_cli.utils import AliasParam
from alvtime_cli.local_service import LocalService
from alvtime_cli import model
from alvtime_cli.utils import style_time_entry


@click.command(help="Starts an activity")
@click.option("--at", type=datetime)
@click.argument("alias", type=AliasParam)
@click.argument("comment", required=False)
@click.pass_context
def start(ctx, alias: model.TaskAlias, at: datetime = None, comment: str = None):
    service = cast(LocalService, ctx.obj)

    current_entry = service.current_entry()
    if current_entry:
        raise click.exceptions.ClickException("Project already running")

    time_entry = service.start(
        task_id=alias.task_id,
        at=at,
        comment=comment)

    click.echo(style_time_entry(time_entry))
