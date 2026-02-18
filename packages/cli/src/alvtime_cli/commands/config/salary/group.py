import click
from .set import set_


@click.group(name="salary", help="Commands to handle you salary config")
def group():
    pass


group.add_command(set_)
