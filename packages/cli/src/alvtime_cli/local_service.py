from datetime import datetime

from alvtime_cli import config, model
from alvtime_cli.repo import Repo
from alvtime_cli.alvtime_client import AlvtimeClient


class TaskAlreadyStartedError(Exception):
    pass


class TaskNotRunningError(Exception):
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

    def start(self, task_id: int, at: datetime, comment: str) -> model.TimeEntry:
        # Check if we're already started
        if self.current_entry():
            raise TaskAlreadyStartedError()

        # Create the new entry
        entry = model.TimeEntry(
            task_id=task_id,
            start=at or datetime.now().astimezone(),
            comment=comment)

        # Make sure the start time is rounded (down) to nearest second
        entry.start = entry.start.replace(microsecond=0)

        # Store it
        self.repo.insert_time_entry(entry)

        return entry

    def restart(self, at, comment):
        raise NotImplementedError()

    def stop(self, at, comment):
        # Find current entry
        current_entry = self.current_entry()

        # Bail out if we didn't find anything
        if not current_entry:
            raise TaskAlreadyStartedError()

        # Verify stop time
        if not at:
            at = datetime.now().astimezone()
        if at < current_entry.start:
            raise ValueError("Stop time cannot be before start time")

        # Make sure the stop time is rounded (down) to nearest second
        at = at.replace(microsecond=0)

        # Close the entry
        delta = at - current_entry.start
        current_entry.duration = delta.total_seconds()

        # And save it
        return current_entry

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
