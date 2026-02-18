import click
from alvtime_cli import config

@click.command(name="set", help="Set config for yearly salary")
def set_():
    click.secho("Enter you yearly salary, for calculations of timebank monetary value.",
                fg="green")

    salary = click.prompt("Salary", hide_input=False, type=int)
    config.set(config.Keys.salary, salary)

    click.echo()
    click.secho("Yay 💰", fg="bright_white", bold=True)
