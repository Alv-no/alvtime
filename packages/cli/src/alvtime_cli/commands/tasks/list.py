import click
from typing import cast
from ...utils import handle_exceptions, style
from ...alvtime_client import AlvtimeClient
from ...local_service import LocalService


@click.command(name="list", help="Lists all available tasks")
@click.option("-l", "--include-locked", is_flag=True)
@click.argument("search", required=False)
@click.pass_context
@handle_exceptions()
def list_(ctx: click.Context, include_locked: bool, search: str = None):
    service = cast(LocalService, ctx.obj)

    #TODO: replace the below with a call through the service

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
