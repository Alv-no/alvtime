from typing import cast

import click
from datetime import timedelta

from alvtime_cli.local_service import LocalService
from alvtime_cli.config import ConfigKeyError


@click.command(name="show", help="Display available timebank hours, breakdown and total")
@click.option("--raw", is_flag=True, help="Show hour total in raw form")
@click.option(
    "--me-the-money",
    "show_money",
    is_flag=True,
    help="Display timebank in monetary value",
)
@click.pass_context
def show(ctx: click.Context, raw: bool, show_money: bool) -> None:
    service = cast(LocalService, ctx.obj)
    available_hours = service.get_available_hours()

    if not available_hours.entries:
        click.echo("No timebank entries available")
        return

    # Format with Norwegian number format (space thousands separator, comma decimal)
    def format_currency(value: float) -> str:
        formatted = f"{int(value):,}".replace(",", " ")
        return f"{formatted},-"

    # Format hours with decimals only when needed
    def format_hours(hours: float) -> str:
        """Format hours with decimals only when needed"""
        if hours % 1 == 0:  # Check if it's a whole number
            return f"{hours:>6.0f}h"
        elif hours * 10 % 1 == 0:  # Check if only one decimal place
            return f"{hours:>6.1f}h"
        else:
            return f"{hours:>6.2f}h"

    if raw and not show_money:
        click.echo(available_hours.available_hours_before_compensation)

    if show_money and raw:
        # Calculate monetary value based on salary
        try:
            total_hours_with_compensation = service.get_total_hours_with_compensation(available_hours.entries)
            total_value = int(service.calculate_monetary_timebank(timedelta(hours=total_hours_with_compensation)))

            click.echo(total_value)

        except ConfigKeyError as e:
            click.secho(f"Error: {e}", fg="red")
            click.echo("Use 'alvtime config salary set' to configure salary.")


    if not raw:
            # Show breakdown by compensation rate
            unspent_overtime = service.calculate_unspent_overtime(available_hours.entries)

            compensation_labels = {
                "0.5": "Frivillig (Volunteer)",
                "1.0": "Interntid (Mandatory)",
                "1.5": "Fakturerbart (Billable)",
                "2.0": "Tommy Time (Mandatory Billable)",
            }

            click.secho("Breakdown by compensation rate:", fg="green")
            total = f"{' ':<2}{'Total':.<35}{format_hours(available_hours.available_hours_before_compensation)}"
            total_money = 0
            for rate, label in compensation_labels.items():
                hours = unspent_overtime.get(rate, 0)
                if hours == 0:
                    continue

                output = f"{' ':<2}{label:.<35}{format_hours(hours)}"

                if show_money:
                    try:
                        rate_multiplier = float(rate)
                        total_hours_with_rate = hours * rate_multiplier
                        value = int(service.calculate_monetary_timebank(timedelta(hours=total_hours_with_rate)))
                        output += f" = {format_currency(value):>10}"
                        total_money += value
                    except ConfigKeyError:
                        # Silently skip monetary display for breakdown if salary not configured
                        pass
                    except TypeError:
                        pass

                click.echo(output)
            if show_money:
                total += f" = {format_currency(total_money):>10}"



            click.echo("  " + "-"*(len(total)-2))
            click.echo(total)


