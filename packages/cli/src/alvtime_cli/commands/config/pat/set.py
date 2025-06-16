import click
from alvtime_cli import config


@click.command(name="set")
def set_():
    click.secho("Create a personal access token at https://alvtime.no/#/tokens.",
                fg="green")

    pat = click.prompt("Personal access token", hide_input=True)
    config.set(config.Keys.personal_access_token, pat)

    click.echo()
    click.secho("Yay 🎉", fg="bright_white", bold=True)
