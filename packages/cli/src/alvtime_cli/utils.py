from click.shell_completion import CompletionItem
from functools import partial, wraps
from requests import HTTPError, ConnectionError
from typing import cast
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
     "name":     {"fg": "magenta"}
}
FALLBACK_COLORS = {}


def style(message: str, main_class: str, extra: dict = {}) -> str:
    colors = COLOR_MAP.get(main_class, FALLBACK_COLORS)
    return click.style(message, **{**colors, **extra})


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
