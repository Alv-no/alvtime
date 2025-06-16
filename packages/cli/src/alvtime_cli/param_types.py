from datetime import datetime, date
from click.shell_completion import CompletionItem
from typing import cast
import arrow
import click

from alvtime_cli import config, model
from alvtime_cli.local_service import LocalService


class AliasParamType(click.ParamType):
    name = "alias"

    def convert(self, value, param, ctx) -> model.TaskAlias:
        if isinstance(value, model.TaskAlias):
            return value

        service = cast(LocalService, ctx.obj)

        aliases = service.get_aliases()
        alias = next((a for a in aliases if a.name == value), None)
        if not alias:
            self.fail(f"{value!r} is not a known alias", param, ctx)

        return alias

    def shell_complete(self, ctx, param, incomplete):
        alias_names = config.get(config.Keys.task_aliases).values()
        return [CompletionItem(name) for name in alias_names if name.startswith(incomplete)]


AliasParam = AliasParamType()


class DateTimeParamType(click.ParamType):
    name = "datetime"

    def convert(self, value, param, ctx) -> datetime:
        if isinstance(value, datetime):
            return value

        formats = ["YYYY-MM-DD HH:mm:ss", "YYYY-MM-DD HH:mm", "HH:mm:ss", "HH:mm"]

        ret = arrow.now("local").replace(second=0, microsecond=0)
        for format in formats:
            try:
                parsed = arrow.get(value, format)
                ret = ret.replace(hour=parsed.hour,
                                  minute=parsed.minute,
                                  second=parsed.second)
                if "YYYY" in format:
                    ret = ret.replace(year=parsed.year)
                if "MM" in format:
                    ret = ret.replace(month=parsed.month)
                if "DD" in format:
                    ret = ret.replace(day=parsed.day)
                return ret.datetime
            except arrow.parser.ParserMatchError:
                pass

        self.fail("Unable to parse datetime")


DateTimeParam = DateTimeParamType()


class DateParamType(click.ParamType):
    name = "date"

    def convert(self, value, param, ctx) -> date:
        if isinstance(value, date):
            return value

        formats = ["YYYY-MM-DD", "MM-DD", "DD"]

        ret = arrow.now("local").replace(second=0, microsecond=0)
        for format in formats:
            try:
                parsed = arrow.get(value, format)
                ret = ret.replace(hour=parsed.hour,
                                  minute=parsed.minute,
                                  second=parsed.second)
                if "YYYY" in format:
                    ret = ret.replace(year=parsed.year)
                if "MM" in format:
                    ret = ret.replace(month=parsed.month)
                if "DD" in format:
                    ret = ret.replace(day=parsed.day)
                return ret.date()
            except arrow.parser.ParserMatchError:
                pass

        self.fail("Unable to parse date")


DateParam = DateParamType()
