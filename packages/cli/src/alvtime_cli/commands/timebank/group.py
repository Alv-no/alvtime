import click
from .show import show


@click.group(name="timebank", help="Commands to show timebank information")
def group():
    pass


group.add_command(show)
