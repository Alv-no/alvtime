from datetime import datetime, timedelta
from click.shell_completion import CompletionItem
from functools import partial, wraps
from requests import HTTPError, ConnectionError
from typing import cast
import arrow
import click
import sys

from alvtime_cli import config, model
from alvtime_cli.local_service import LocalService


def handle_exceptions(func=None):
    if not func:
        return partial(handle_exceptions)

    @wraps(func)
    def wrapper(*args, **kwargs):
        try:
            return func(*args, **kwargs)
        except HTTPError as e:
            click.secho(e, fg="red", err=True)
            click.secho(e.response.content.decode("utf-8"), err=True)
            if e.response.status_code == 401:
                click.secho("Use 'alvtime pat set' to update your personal access token.", err=True)
            sys.exit(1)
        except ConnectionError as e:
            click.secho(e, fg="red", err=True)
            sys.exit(1)
        except Exception as e:
            click.secho(e, fg="red", err=True)
            sys.exit(1)

    return wrapper


# Reference: https://click.palletsprojects.com/en/stable/api/#click.style
COLOR_MAP = {
     "customer": {"fg": "yellow"},
     "project":  {"fg": "green"},
     "task":     {"fg": "bright_blue"},
     "rate":     {"fg": "white", "dim": True},
     "name":     {"fg": "magenta"},
     "time":     {"fg": "green"},
     "comment":  {"fg": "white", "dim": True},
}
FALLBACK_COLORS = {}


def style(message: str, main_class: str, extra: dict = {}) -> str:
    colors = COLOR_MAP.get(main_class, FALLBACK_COLORS)
    return click.style(message, **{**colors, **extra})


def style_time_entry(time_entry: model.TimeEntry) -> str:
    start_humanized = arrow.Arrow.fromdatetime(time_entry.start).humanize()
    ret = [
        "Project ",
        style(time_entry.task_id, "task"),
        " started ",
        style(f"{start_humanized} ", "time")]

    if time_entry.duration:
        duration = timedelta(seconds=time_entry.duration)
        stop_time = time_entry.start + duration
        stop_humanized = arrow.Arrow.fromdatetime(stop_time).humanize()
        total_minutes, _ = divmod(duration.total_seconds(), 60)
        hour, minute = divmod(total_minutes, 60)

        ret.extend([
            "and stopped ",
            style(f"{stop_humanized} ", "time"),
            style(f"({int(hour):02}:{int(minute):02} elapsed)", "comment")
        ])
    else:
        ret.extend([
            style(f"({time_entry.start.strftime("%Y-%m-%d %H:%M")})", "comment")
        ])
    return ''.join(ret)


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
