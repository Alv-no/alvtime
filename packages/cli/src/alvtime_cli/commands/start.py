from datetime import datetime
from typing import cast
import click

from alvtime_cli.param_types import AliasParam, DateTimeParam
from alvtime_cli.local_service import LocalService, TaskAlreadyStartedError
from alvtime_cli import model
from alvtime_cli.utils import style_time_entry


@click.command(help="Starts an activity")
@click.option("--at", type=DateTimeParam)
@click.argument("alias", type=AliasParam)
@click.argument("comment", required=False)
@click.pass_context
def start(ctx, alias: model.TaskAlias, at: datetime = None, comment: str = None):
    service = cast(LocalService, ctx.obj)

    try:
        time_entry = service.start(
            task_id=alias.task.id,
            at=at,
            comment=comment)
    except TaskAlreadyStartedError:
        raise click.exceptions.ClickException("Project already running")

    click.echo(style_time_entry(time_entry))
