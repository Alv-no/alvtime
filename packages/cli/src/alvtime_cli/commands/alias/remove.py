import click
import click.shell_completion

from alvtime_cli import config
from alvtime_cli.param_types import AliasParam
from alvtime_cli import model


def _remove_alias(name: str):
    aliases = config.get(config.Keys.task_aliases, {})

    if name not in aliases.values():
        raise click.ClickException(f"No alias found with name '{name}'")

    aliases = {k: v for k, v in aliases.items() if v != name}
    config.set(config.Keys.task_aliases, aliases)


@click.command()
@click.argument("alias", type=AliasParam)
def remove(alias: model.TaskAlias):
    """
    Removes a task alias

    Example:

    $ alvtime alias remove alvops
    """
    _remove_alias(alias.name)
