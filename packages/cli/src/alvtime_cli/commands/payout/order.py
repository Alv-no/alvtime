from typing import cast

import click
from requests import HTTPError
import sys

from alvtime_cli.local_service import LocalService
from alvtime_cli.utils import handle_exceptions


@click.command(name="order", help="Order payout of selected amount of available hours")
@click.option("--all", is_flag=True, help="Order payout of all available hours")
@click.argument("amount", type=float, required=False)
@click.pass_context
@handle_exceptions()
def order(ctx: click.Context, all: bool, amount: float | None) -> None:
    service = cast(LocalService, ctx.obj)
    available_amount = service.get_available_hours().available_hours_before_compensation

    if not all and amount is None:
        raise click.exceptions.ClickException("You need to provide how many hours you want payed out")

    def _order(hours: float):
        try:
            return service.order_payout(hours)
        except HTTPError as error:
            response = error.response
            if response is None:
                click.secho("No response from server.", fg="red", err=True)
                sys.exit(1)

            title: str | None = None
            message: str | None = response.text.strip() if response.text else None
            first_error: str | None = None
            printed_messages: set[str] = set()
            try:
                payload = response.json()
            except ValueError:
                payload = None

            if isinstance(payload, dict):
                title = payload.get("title") or title
                message = payload.get("detail") or message
                if title:
                    click.secho(title, fg="red", err=True)
                errors = payload.get("errors")
                if isinstance(errors, dict):
                    for value in errors.values():
                        if isinstance(value, (list, tuple)):
                            for item in value:
                                if not item:
                                    continue
                                text = str(item)
                                if first_error is None:
                                    first_error = text
                                printed_messages.add(text)
                                click.secho(text, fg="red", err=True)
                        elif value:
                            text = str(value)
                            if first_error is None:
                                first_error = text
                            printed_messages.add(text)
                            click.secho(text, fg="red", err=True)
                    if first_error:
                        message = first_error
            elif title:
                click.secho(title, fg="red", err=True)

            if message and message not in printed_messages:
                click.secho(message, fg="red", err=True)
                printed_messages.add(message)

            if not printed_messages:
                fallback = message or str(error)
                click.secho(fallback, fg="red", err=True)

            sys.exit(1)

    if not all and amount is not None:
        if amount > available_amount:
            click.secho(f"Unable to order {amount}h, you only have {available_amount}h available", fg="red")
            return
        response = _order(amount)
        ordered_hours = response.hours
        click.secho(f"Ordered payout of {ordered_hours}h", fg="green")
        return

    if all:
        response = _order(available_amount)
        ordered_hours = response.hours
        click.secho(f"Ordered payout of {ordered_hours}h", fg="green")
        return
