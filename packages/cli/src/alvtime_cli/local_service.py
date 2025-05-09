from datetime import datetime

from alvtime_cli import config, model
from alvtime_cli.repo import Repo
from alvtime_cli.alvtime_client import AlvtimeClient


class TaskAlreadyStartedError(Exception):
    pass


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

    def start(self, task_id: int, at: datetime, comment: str):
        # Check if we're already started
        entries = self.repo.find_open_entries()
        if entries:
            raise TaskAlreadyStartedError()

        # Create the new entry
        entry = model.TimeEntry(
            task_id=task_id,
            start=at or datetime.now().astimezone(),
            comment=comment)

        # Store it
        self.repo.insert_time_entry(entry)

    def restart(self, at, comment):
        raise NotImplementedError()

    def current_entry(self) -> model.TimeEntry | None:
        """
        Returns the current 'open' time entry if any
        """
        open_entries = self.repo.find_open_entries()
        if not open_entries:
            return None
        if len(open_entries) > 1:
            raise RuntimeError("More than one time entry is currently open. You know who to call!")
        return open_entries[0]
