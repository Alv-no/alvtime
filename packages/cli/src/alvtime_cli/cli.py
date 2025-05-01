import click
from .commands import ping, completion
from .commands.pat import group as pat_group
from .commands.tasks import group as tasks_group
from .commands.alias import group as alias_group
from .local_service import LocalService
from .alvtime_client import AlvtimeClient
from .repo import Repo


@click.group()
@click.pass_context
def main(ctx: click.Context):
    alvtime_client = AlvtimeClient()
    repo = Repo()
    service = LocalService(
        repo=repo,
        alvtime_client=alvtime_client)
    ctx.obj = service


main.add_command(completion.completion)
main.add_command(ping.ping)
main.add_command(alias_group.group)
main.add_command(pat_group.group)
main.add_command(tasks_group.group)


if __name__ == "__main__":
    main()
