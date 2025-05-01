import click
from ...utils import handle_exceptions
from ...alvtime_client import AlvtimeClient


@click.command(name="list", help="Lists all available tasks")
@click.option("-l", "--include-locked", is_flag=True)
@click.argument("search", required=False)
@handle_exceptions()
def list_(include_locked: bool, search: str = None):
    alvtime_client = AlvtimeClient()

    customers = alvtime_client.list_customers(include_locked)
    for customer in customers:
        for project in customer.projects:
            for task in project.tasks:
                if search:
                    search_string = f"{customer.name} {project.name} {task.name} {task.id}".lower()
                    if not search.lower() in search_string:
                        continue

                if task.locked:
                    fg_proj = "white"
                    fg_cust = "white"
                    fg_task = "white"
                else:
                    fg_proj = "green"
                    fg_cust = "yellow"
                    fg_task = "bright_blue"

                click.secho(f"{customer.name} ", fg=fg_cust, nl=False)
                click.secho(f"{project.name} ", fg=fg_proj, nl=False)
                click.secho(f"{task.name} ({task.id}) ", fg=fg_task, nl=False)
                click.echo(f"({task.rate})")
