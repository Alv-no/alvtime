import click
from typing import cast

from alvtime_cli.local_service import LocalService
from alvtime_cli.utils import style, style_task


@click.command(name="list")
@click.pass_context
def list_(ctx: click.Context):
    """
    List all existing task aliases
    """
    service = cast(LocalService, ctx.obj)
    aliases = service.get_aliases()
    for alias in aliases:
        click.echo(style(alias.name, "name"), nl=False)
        click.echo(": ", nl=False)
        click.echo(style_task(alias.task, True))
