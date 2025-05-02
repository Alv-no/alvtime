import click

from alvtime_cli import config


def _remove_alias(name: str):
    aliases = config.get(config.Keys.task_aliases, {})

    if name not in aliases.values():
        raise click.ClickException(f"No alias found with name '{name}'")
    
    aliases = { k:v for k,v in aliases.items() if v != name }
    config.set(config.Keys.task_aliases, aliases)

@click.command()
@click.argument("name", type=str)
def remove(name: str):
    """
    Removes a task alias

    Example:

    $ alvtime alias remove alvops
    """
    _remove_alias(name)
