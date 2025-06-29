from enum import StrEnum
from pathlib import Path
import os
import yaml


config_filename = os.getenv("ALVTIME_CONFIG",
                            Path.home() / ".alvtime.conf")


class Keys(StrEnum):
    personal_access_token = "personalAccessToken"
    database_path = "databasePath"
    alvtime_base_url = "alvtimeBaseUrl"
    task_aliases = "taskAliases"
    auto_sync = "autoSync"


defaults = {
    Keys.database_path: Path.home() / ".alvtime.db",
    Keys.alvtime_base_url: "https://api.alvtime.no",
    Keys.task_aliases: {},
    Keys.auto_sync: False
}


def _load():
    try:
        with open(config_filename, "r") as f:
            data = yaml.safe_load(f)
    except FileNotFoundError:
        return {}
    if not isinstance(data, dict):
        data = {}
    return data


def _save(config):
    with open(config_filename, "w") as f:
        yaml.dump(config, f)


def get(key, default=None):
    config = _load()
    key = str(key)
    if default is not None:
        return config.get(key, default)

    if key in defaults:
        return config.get(key, defaults[key])
    else:
        return config[key]


def set(key: str, value):
    config = _load()
    config[str(key)] = value
    _save(config)
