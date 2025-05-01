import click


@click.command()
@click.argument("name", type=str)
def remove():
    """
    Removes a task alias

    Example:

    $ alvtime alias remove alvops
    """
    pass
