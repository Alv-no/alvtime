import click
from ... import config


@click.command(name="set")
def set_():
    click.secho("Create a personal access token at https://alvtime.no/#/tokens.", fg="green")

    pat = click.prompt("Personal access token", hide_input=True)
    config.set("pat", pat)

    click.echo()
    click.secho("Yay 🎉", fg="bright_white", bold=True)
