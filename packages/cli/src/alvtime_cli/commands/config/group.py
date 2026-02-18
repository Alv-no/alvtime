import click
from .auto_break import group as auto_break_group
from .pat import group as pat_group
from .salary import group as salary_group
from . import auto_sync


@click.group(name="config", help="CLI configuration")
def group():
    pass


group.add_command(auto_sync.auto_sync)
group.add_command(auto_break_group.group)
group.add_command(pat_group.group)
group.add_command(salary_group.group)
