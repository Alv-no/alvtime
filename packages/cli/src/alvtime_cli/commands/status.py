import click
from typing import cast

from alvtime_cli.local_service import LocalService


@click.command(help="Display current status")
@click.pass_context
def status(ctx):
    service = cast(LocalService, ctx.obj)
    print(repr(service.status()))
    click.echo(service.status())
