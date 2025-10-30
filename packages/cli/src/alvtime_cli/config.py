from enum import StrEnum
from pathlib import Path
import datetime
import os
import pydantic
import yaml


class ConfigKeyError(Exception):
    """Raised when a required configuration key is missing"""
    pass


config_filename = os.getenv("ALVTIME_CONFIG",
                            Path.home() / ".alvtime.conf")


class Weekday(StrEnum):
    mon = "mon"
    tue = "tue"
    wed = "wed"
    thu = "thu"
    fri = "fri"
    sat = "sat"
    sun = "sun"

    @classmethod
    def from_date(cls, date: datetime.date):
        return list(cls)[date.isoweekday() - 1]


class AutoBreak(pydantic.BaseModel):
    comment: str = ""
    weekdays: list[Weekday]
    start: datetime.time
    stop: datetime.time

    model_config = {"extra": "forbid"}


class Keys(StrEnum):
    personal_access_token = "personalAccessToken"
    database_path = "databasePath"
    alvtime_base_url = "alvtimeBaseUrl"
    task_aliases = "taskAliases"
    auto_sync = "autoSync"
    salary = "salary"


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
        if key not in config:
            raise ConfigKeyError(f"Missing required configuration key: '{key}'")
        return config[key]


def set(key: str, value):
    config = _load()
    config[str(key)] = value
    _save(config)
