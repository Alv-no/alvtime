import click
from .commands import check, completion, log, ping, pull, restart, start, status, stop, sync
from .commands.config import group as config_group
from .commands.tasks import group as tasks_group
from .commands.alias import group as alias_group
from .commands.breaks import group as break_group
from .commands.timebank import group as timebank_group
from .local_service import LocalService
from .alvtime_client import AlvtimeClient
from .repo import Repo
from . import config


@click.group()
@click.option("-c", "--config", "config_filename",
              metavar="FILENAME",
              type=str,
              envvar="ALVTIME_CONFIG",
              help="Specify which configuration file to use. Defaults to ~/.alvtime.conf. "
                   "Can also be set using the ALVTIME_CONFIG environment variable.")
@click.pass_context
def main(ctx: click.Context, config_filename):
    if config_filename:
        config.config_filename = config_filename
    alvtime_client = AlvtimeClient()
    repo = Repo(config.get(config.Keys.database_path))
    service = LocalService(
        repo=repo,
        alvtime_client=alvtime_client)
    ctx.obj = service


main.add_command(check.check)
main.add_command(completion.completion)
main.add_command(log.log)
main.add_command(ping.ping)
main.add_command(pull.pull)
main.add_command(restart.restart)
main.add_command(start.start)
main.add_command(status.status)
main.add_command(stop.stop)
main.add_command(sync.sync)
main.add_command(alias_group.group)
main.add_command(break_group.group)
main.add_command(config_group.group)
main.add_command(tasks_group.group)
main.add_command(timebank_group.group)


if __name__ == "__main__":
    main()
