import click
from datetime import datetime


@click.command(help="Restarts last activity")
@click.option("--at", type=datetime)
def restart(at: datetime = None):
    raise NotImplementedError()
