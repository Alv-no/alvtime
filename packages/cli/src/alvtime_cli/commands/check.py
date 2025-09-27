import click
from datetime import date
from typing import cast

from alvtime_cli.local_service import LocalService
from alvtime_cli.param_types import DateParam
from alvtime_cli.utils import style_check_result, style
from alvtime_cli.model import CheckResultType


@click.command(help="Checks that hours has been registered correctly")
@click.option("--from", "from_", type=DateParam, default=date.today())
@click.option("--to", type=DateParam, default=date.today())
@click.pass_context
def check(ctx, from_: date, to: date):
    service = cast(LocalService, ctx.obj)
    check_results = service.check(from_, to)

    result_count = {"ok": 0,
                    "warning": 0,
                    "error": 0}
    for result in check_results:
        result_count[result.result_type] += 1
        if not result.result_type == CheckResultType.ok:
            click.echo(style_check_result(result))

    summary_parts = []
    if result_count["warning"]:
        summary_parts.append(style(f"{result_count['warning']} warnings", "warning"))
    if result_count["error"]:
        summary_parts.append(style(f"{result_count['error']} errors", "error"))

    if summary_parts:
        click.echo()
        click.echo("Summary: " + "  ".join(summary_parts))
