import click
from typing import cast

from alvtime_cli.local_service import LocalService
from alvtime_cli.utils import style_time_entry


@click.command(help="Display current status")
@click.pass_context
def status(ctx):
    service = cast(LocalService, ctx.obj)
    current_entry = service.current_entry()

    if not current_entry:
        click.echo("No project started.")
        return

    click.echo(style_time_entry(current_entry))
