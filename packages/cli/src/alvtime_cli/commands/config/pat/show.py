import click
import sys
from alvtime_cli import config


@click.command()
def show():
    try:
        click.echo(config.get(config.Keys.personal_access_token))
    except KeyError:
        click.secho("ERROR: No PAT registered", fg="red", err=True)
        sys.exit(1)
