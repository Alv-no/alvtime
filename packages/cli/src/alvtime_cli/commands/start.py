import click
from datetime import datetime


@click.command(help="Starts an activity")
@click.option("--at", type=datetime)
@click.argument("alias")
@click.argument("comment", required=False)
def start(alias: str, at: datetime = None, comment: str = None):
    raise NotImplementedError()
