import click
from .order import order


@click.group(name="payout", help="Commands to order payout")
def group():
    pass

group.add_command(order)
