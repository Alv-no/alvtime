import click
from .list import list_
from .add import add
from .remove import remove


@click.group(name="alias", help="Manage task aliases")
def group():
    pass


group.add_command(list_)
group.add_command(add)
group.add_command(remove)
