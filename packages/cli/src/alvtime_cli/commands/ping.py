import click
from ..alvtime_client import AlvtimeClient


@click.command(help="Pings the Alvtime API server")
def ping():
    alvtime_client = AlvtimeClient()
    click.echo(alvtime_client.ping())
