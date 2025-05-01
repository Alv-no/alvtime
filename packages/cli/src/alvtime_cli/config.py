from pathlib import Path
import yaml


config_filename = Path.home() / ".alvtime.conf"


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
    if default is None:
        return config[key]
    else:
        return config.get(key, default)


def set(key: str, value):
    config = _load()
    config[key] = value
    _save(config)
