import click
from .edit import edit


@click.group(name="auto-break", help="Auto-break configuration")
def group():
    pass


group.add_command(edit)
