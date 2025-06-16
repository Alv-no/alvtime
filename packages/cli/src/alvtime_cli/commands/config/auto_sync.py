import click

from alvtime_cli import config


@click.command()
@click.argument("value", type=bool, required=True)
@click.pass_context
def auto_sync(ctx, value: bool):
    """
    Automatic sync synchronizes the day any time 'alvtime stop' is invoked
    """

    config.set(config.Keys.auto_sync, value)

    if value:
        click.echo(f"auto-sync {click.style('enabled', 'green')}")
    else:
        click.echo(f"auto-sync {click.style('disabled', 'red')}")
