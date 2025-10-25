from collections import defaultdict
from datetime import timedelta
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
     "date":     {"fg": "white", "dim": True},
     "comment":  {"fg": "white", "dim": True},
     "warning":  {"fg": "yellow"},
}
FALLBACK_COLORS = {}


def style(message: str, main_class: str, extra: dict = {}) -> str:
    if message is None:
        return ""
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


def style_check_result(check_result: model.CheckResult, column_delimiter="  ") -> str:
    ret = []
    ret.append(style(check_result.date.isoformat(), "date"))
    hours = check_result.registered_duration.total_seconds() / 3600
    ret.append(column_delimiter)
    ret.append(style(f"{hours:.1f} hours", "time"))
    ret.append(column_delimiter)
    if check_result.result_type == model.CheckResultType.ok:
        ret.append(style(check_result.message, "ok"))
    elif check_result.result_type == model.CheckResultType.warning:
        ret.append(style(check_result.message, "warning"))
    elif check_result.result_type == model.CheckResultType.error:
        ret.append(style(check_result.message, "error"))
    return ''.join(ret)


def group_by(items, predicate):
    grouped = defaultdict(list)
    for item in items:
        key = predicate(item)
        grouped[key].append(item)
    return dict(grouped)


def iterate_dates(start_date, end_date):
    current = start_date
    while current <= end_date:
        yield current
        current += timedelta(days=1)


def _breakify_entry(entry: model.TimeEntry, _break: model.TimeBreak) -> list[model.TimeEntry]:
    """
    Applies a break to a time entry, returning zero
    one or two entries.
    """
    # Calculate start and stop times
    entry_start = entry.start
    entry_stop = entry.start + entry.duration
    break_start = _break.start
    break_stop = _break.start + _break.duration

    # No overlap if break starts after entry stopped
    if break_start >= entry_stop:
        return [entry]

    # No overlap if break stopped before entry starts
    if break_stop <= entry_start:
        return [entry]

    # Empty result of entry is fully contained in a break
    if break_start <= entry_start and break_stop >= entry_stop:
        return []

    # Check if break is overlapping start
    if break_start <= entry_start:
        return [entry.model_copy(update={
            "start": break_stop,
            "duration": (entry_stop - break_stop)})]

    # Check if break is overlapping end
    if break_stop >= entry_stop:
        return [entry.model_copy(update={
            "duration": (break_start - entry_start)})]

    # If we're here, the break is in the middle
    return [
        entry.model_copy(update={
            "duration": (break_start - entry_start)}),
        entry.model_copy(update={
            "start": break_stop,
            "duration": (entry_stop - break_stop)})]


def entries_overlap(e1: model.BaseEntry, e2: model.BaseEntry) -> bool:
    e1_stop = e1.start + e1.duration
    e2_stop = e2.start + e2.duration

    return e1.start < e2_stop and e1_stop > e2.start


def breakify_entries(entries: list[model.TimeEntry],
                     breaks: list[model.TimeBreak]) -> list[model.TimeEntry]:
    """
    Applies a list of breaks to a list of time entries, returning a new
    list of time entries outside the break intervals.

    The number of entries returned can be both higher and lower than the
    entries provided.
    """
    # Start with all entries (make a copy to not mess up the caller)
    ret = list(entries)

    for _break in breaks:
        ret = [e for entry in ret for e in _breakify_entry(entry, _break)]

    return ret
