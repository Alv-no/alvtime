from .repo import Repo
from .alvtime_client import AlvtimeClient
from . import model


class LocalService:
    def __init__(self, repo: Repo, alvtime_client: AlvtimeClient):
        self.repo = repo
        self.alvtime_client = alvtime_client

    def get_all_tasks(self, include_locked=False) -> list[model.Task]:
        return self.alvtime_client.list_tasks(include_locked)
