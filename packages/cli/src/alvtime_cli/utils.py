from functools import partial, wraps
from requests import HTTPError, ConnectionError
import click
import sys


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
