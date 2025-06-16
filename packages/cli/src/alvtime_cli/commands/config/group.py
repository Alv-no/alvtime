import click
from .pat import group as pat_group
from . import auto_sync


@click.group(name="config", help="CLI configuration")
def group():
    pass


group.add_command(auto_sync.auto_sync)
group.add_command(pat_group.group)
