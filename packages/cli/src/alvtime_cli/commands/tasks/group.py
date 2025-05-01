import click
from .list import list_


@click.group(name="tasks", help="Commands to list and search Alvtime tasks")
def group():
    pass


group.add_command(list_)
