import click
from .add import add


@click.group(name="break", help="Manage breaks")
def group():
    pass


group.add_command(add)
