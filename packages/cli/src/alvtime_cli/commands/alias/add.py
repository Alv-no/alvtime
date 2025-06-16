import click
from typing import cast
from alvtime_cli.local_service import LocalService


@click.command()
@click.argument("name", type=str)
@click.argument("task_id", type=int)
@click.pass_context
def add(ctx: click.Context, name: str, task_id: int):
    """
    Add a task alias

    Task aliases are used when interacting with the
    alvtime cli to easily specify the exact task you're
    working on.

    Find the correct task_id by running "alvtime tasks list":

    \b
    $ alvtime tasks list alvops
    109 - Alv Interntid 50% AlvOps (50%)
      ↖
       task_id

    Example:

    \b
    $ alvtime alias add alvops 109

    """
    service = cast(LocalService, ctx.obj)

    service.add_task_alias(
            name=name,
            task_id=task_id)
