import click

from alvtime_cli import config
from alvtime_cli.model import TaskAlias
from alvtime_cli.utils import style


def _get_aliases() -> dict[TaskAlias]:
    raw_aliases = config.get(config.Keys.task_aliases, {})
    return [
        TaskAlias(task_id=int(task_id), name=name)
        for task_id, name in raw_aliases.items()
    ]


@click.command(name="list")
def list_():
    """
    List all existing task aliases
    """
    aliases = _get_aliases()
    for alias in aliases:
        click.echo(
            f"{style(alias.task_id, "task")}: "
            f"{style(alias.name, "name")}"
        )
