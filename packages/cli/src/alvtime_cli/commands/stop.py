import click
from datetime import datetime


@click.command(help="Stops current activity")
@click.option("--at", type=datetime)
def stop(at: datetime = None):
    raise NotImplementedError()
