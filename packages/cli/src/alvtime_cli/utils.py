from collections import defaultdict
from functools import partial, wraps
from requests import HTTPError, ConnectionError
import arrow
import click
import sys

from alvtime_cli import model


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
        style_task(time_entry.task),
        " started ",
        style(f"{start_humanized} ", "time")]

    if time_entry.duration:
        stop_time = time_entry.start + time_entry.duration
        stop_humanized = arrow.Arrow.fromdatetime(stop_time).humanize()
        total_minutes, _ = divmod(time_entry.duration.total_seconds(), 60)
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


def style_task(task: model.Task, with_id=False) -> str:
    ret = []
    if with_id:
        ret.append("[")
        ret.append(style(task.id, "task"))
        ret.append("] ")
    ret.append(style(task.customer_name, "customer"))
    ret.append(" ")
    ret.append(style(task.project_name, "project"))
    ret.append(" ")
    ret.append(style(task.name, "task"))
    ret.append(" ")
    ret.append(style(f"({str(task.rate*100)}%)", "rate"))
    return "".join(ret)


def group_by(items, predicate):
    grouped = defaultdict(list)
    for item in items:
        key = predicate(item)
        grouped[key].append(item)
    return dict(grouped)
