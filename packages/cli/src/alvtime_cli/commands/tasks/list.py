import click
from typing import cast
from ...utils import handle_exceptions, style_task
from ...local_service import LocalService


@click.command(name="list", help="Lists all available tasks")
@click.option("-l", "--include-locked", is_flag=True)
@click.option("-f", "--force-refresh", is_flag=True,
              help="Forces refresh of task list from the server")
@click.argument("search", required=False)
@click.pass_context
@handle_exceptions()
def list_(ctx: click.Context, include_locked: bool, force_refresh: bool,
          search: str = None):
    service = cast(LocalService, ctx.obj)

    tasks = service.get_all_tasks(
            include_locked=include_locked,
            force_refresh=force_refresh)

    tasks.sort(key=lambda t: (t.customer_name, t.project_name, t.name, t.id))

    for task in tasks:
        if search:
            search_string = (
                f"{task.customer_name} "
                f"{task.project_name} "
                f"{task.name} "
                f"{task.id}").lower()
            if not search.lower() in search_string:
                continue

        click.echo(style_task(task, True))
