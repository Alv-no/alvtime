import click
from .commands import ping, completion
from .commands.pat import group as pat_group
from .commands.tasks import group as tasks_group
from .commands.alias import group as alias_group


@click.group()
def main():
    pass


main.add_command(completion.completion)
main.add_command(ping.ping)
main.add_command(alias_group.group)
main.add_command(pat_group.group)
main.add_command(tasks_group.group)


if __name__ == "__main__":
    main()
