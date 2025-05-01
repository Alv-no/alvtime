import click
import sys
from ... import config


@click.command()
def show():
    try:
        click.echo(config.get("pat"))
    except KeyError:
        click.secho("ERROR: No PAT registered", fg="red", err=True)
        sys.exit(1)
