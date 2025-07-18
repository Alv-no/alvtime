from dataclasses import dataclass
from datetime import datetime, date, timedelta

from alvtime_cli import config, model
from alvtime_cli.repo import Repo
from alvtime_cli.alvtime_client import AlvtimeClient
from alvtime_cli.utils import group_by


class TaskAlreadyStartedError(Exception):
    pass


class TaskNotRunningError(Exception):
    pass


class TaskNotFoundError(Exception):
    pass


class NoLastEntryError(Exception):
    pass


@dataclass
class PullResult:
    pulled_task_count: int = 0
    local_entries_created: int = 0


@dataclass
class PushResult:
    pushed_time_entries: int = 0
    local_entries_created: int = 0


class LocalService:
    def __init__(self, repo: Repo, alvtime_client: AlvtimeClient):
        self.repo = repo
        self.alvtime_client = alvtime_client

    def get_all_tasks(self, include_locked=False, force_refresh=False) -> list[model.Task]:
        if not force_refresh:
            tasks = self.repo.list_tasks()
        if force_refresh or not tasks:
            tasks = self.alvtime_client.list_tasks()
            for task in tasks:
                self.repo.upsert_task(task)
        if include_locked:
            return tasks
        else:
            return [task for task in tasks if not task.locked]

    def get_aliases(self) -> list[model.TaskAlias]:
        aliases = []
        tasks = self.get_all_tasks()
        for task_id, name in config.get(config.Keys.task_aliases).items():
            task = next((t for t in tasks if t.id == int(task_id)), None)
            if not task:
                raise TaskNotFoundError(f"Task {task_id} not found")
            aliases.append(model.TaskAlias(name=name, task=task))
        return aliases

    def add_task_alias(self, name: str, task_id: int):
        tasks = self.get_all_tasks()
        if not any(True for t in tasks if t.id == task_id):
            raise TaskNotFoundError(f"Task {task_id} not found")

        aliases = config.get(config.Keys.task_aliases, {})
        aliases[str(task_id)] = name
        config.set(config.Keys.task_aliases, aliases)

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

        entry.task = self.repo.find_task(task_id)

        return entry

    def restart(self, at, comment):
        last_entry = self.repo.get_last_time_entry()
        if not last_entry:
            raise NoLastEntryError()
        return self.start(
                task_id=last_entry.task_id,
                at=at,
                comment=comment)

    def stop(self, at, comment):
        # Find current entry
        current_entry = self.current_entry()

        # Bail out if we didn't find anything
        if not current_entry:
            raise TaskNotRunningError()

        # Verify stop time
        if not at:
            at = datetime.now().astimezone()
        if at < current_entry.start:
            raise ValueError("Stop time cannot be before start time")

        # Make sure the stop time is rounded (down) to nearest second
        at = at.replace(microsecond=0)

        # Close the entry
        current_entry.duration = at - current_entry.start

        # Update comment if set
        if comment:
            current_entry.comment = comment

        # And save it
        self.repo.update_time_entry(current_entry)

        # Hydrate task
        current_entry.task = self.repo.find_task(current_entry.task_id)

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
        entry = open_entries[0]

        # Hydrate task
        entry.task = self.repo.find_task(entry.task_id)

        return entry

    def get_entries(self, from_: date, to: date) -> list[model.TimeEntry]:
        # Put all tasks  in a dict
        tasks = {t.id: t for t in self.get_all_tasks()}

        # Load all relevant time entries
        entries = self.repo.list_time_entries(
                from_date=from_,
                to_date=to)

        # Hydrate .task in entries
        for entry in entries:
            entry.task = tasks[entry.task_id]

        return entries

    def round_duration(self, duration: timedelta) -> timedelta:
        total_seconds = duration.total_seconds()
        quarter_hour = 15 * 60
        rounded_seconds = round(total_seconds / quarter_hour) * quarter_hour
        return timedelta(seconds=rounded_seconds)

    def pull(self, from_: date, to: date) -> PullResult:
        # Check if we're already started
        if self.current_entry():
            raise TaskAlreadyStartedError()

        result = PullResult()

        # Fetch all entries from cloud
        cloud_entries = self.alvtime_client.list_time_entries(from_, to)
        result.pulled_task_count = len(cloud_entries)

        # ...and all local entries, grouped by date
        local_entries = group_by(self.repo.list_time_entries(from_, to),
                                 lambda e: e.start.date())

        for cloud_entry in cloud_entries:
            # Check if local entries (rounded) duration adds up to
            # the cloud entry
            entry_date = cloud_entry.start.date()
            local_duration = sum((e.duration
                                  for e in local_entries.get(entry_date, [])
                                  if e.task_id == cloud_entry.task_id), timedelta())
            rounded_local_duration = self.round_duration(local_duration)

            if cloud_entry.duration > rounded_local_duration:
                # More time in the cloud than local. Create a local
                # entry to hold the diff

                # Create the new entry
                new_entry = model.TimeEntry(
                    task_id=cloud_entry.task_id,
                    start=cloud_entry.start.astimezone(),
                    duration=cloud_entry.duration,
                    comment=cloud_entry.comment)

                # Store it
                self.repo.insert_time_entry(new_entry)
                result.local_entries_created += 1

        return result

    def push(self, from_: date, to: date) -> PushResult:
        # Check if we're already started
        if self.current_entry():
            raise TaskAlreadyStartedError()

        result = PushResult()

        # Get all local entries, grouped by date
        local_entries = group_by(self.repo.list_time_entries(from_, to),
                                 lambda e: (e.start.date(), e.task_id))

        to_push = list()
        for (current_date, task_id), entries in local_entries.items():
            # Craft the entry to push
            duration = sum((e.duration for e in entries), timedelta())
            rounded_duration = self.round_duration(duration)
            comment = "\n".join(e.comment for e in entries if e.comment)

            to_push.append(model.TimeEntry(
                task_id=task_id,
                start=current_date,
                duration=rounded_duration,
                comment=comment))

        # Upsert it
        self.alvtime_client.upsert_time_entries(to_push)

        # Count it
        result.pushed_time_entries += len(to_push)

        return result
