import click
from .show import show
from .set import set_


@click.group(name="pat", help="Commands to handle your personal access token")
def group():
    pass


group.add_command(set_)
group.add_command(show)
