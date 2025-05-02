import click
from alvtime_cli.model import TaskAlias
from ... import config


def _add_task_alias(alias: TaskAlias):
    aliases = config.get(config.Keys.task_aliases, {})
    aliases[str(alias.task_id)] = alias.name
    config.set(config.Keys.task_aliases, aliases)


@click.command()
@click.argument("name", type=str)
@click.argument("task_id", type=int)
def add(name: str, task_id: int):
    """
    Add a task alias

    Task aliases are used when interacting with the
    alvtime cli to easily specify the exact task you're
    working on.

    Find the correct task_id by running "alvtime tasks list":

    \b
    $ alvtime tasks list alvops
    109 - Alv Interntid 50% AlvOps (50%)
      ↖
       task_id

    Example:

    \b
    $ alvtime alias add alvops 109

    """
    _add_task_alias(TaskAlias(task_id=task_id, name=name))
