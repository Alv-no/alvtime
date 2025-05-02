from alvtime_cli import config, model
from alvtime_cli.repo import Repo
from alvtime_cli.alvtime_client import AlvtimeClient


class LocalService:
    def __init__(self, repo: Repo, alvtime_client: AlvtimeClient):
        self.repo = repo
        self.alvtime_client = alvtime_client

    def get_all_tasks(self, include_locked=False) -> list[model.Task]:
        return self.alvtime_client.list_tasks(include_locked)

    def get_aliases(self) -> list[model.TaskAlias]:
        return [
            model.TaskAlias(task_id=task_id, name=name)
            for task_id, name in config.get(config.Keys.task_aliases).items()]

    def start(self, task_id, at, comment):
        raise NotImplementedError()

    def restart(self, at, comment):
        raise NotImplementedError()
