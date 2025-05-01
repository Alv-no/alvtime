import click
from ...utils import handle_exceptions, style
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

                click.echo(
                    f"{style(task.id, "task")}: "
                    f"{style(customer.name, "customer")} "
                    f"{style(project.name, "project")} "
                    f"{style(task.name, "task")} " +
                    style(f"({str(task.rate*100)}%)", "rate"))
