import pydantic
from dataclasses import dataclass
from datetime import datetime, date, timedelta

from alvtime_cli import config, model
from alvtime_cli.repo import Repo
from alvtime_cli.alvtime_client import AlvtimeClient
from alvtime_cli.utils import group_by, iterate_dates, breakify_entries, entries_overlap


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
            is_open=True,
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

    def get_entries(self, from_: date, to: date, breakify=False) -> list[model.TimeEntry]:
        # Put all tasks  in a dict
        tasks = {t.id: t for t in self.get_all_tasks()}

        # Load all relevant time entries
        entries = self.repo.list_time_entries(
                from_date=from_,
                to_date=to)

        # Hydrate .task in entries
        for entry in entries:
            entry.task = tasks[entry.task_id]

        # Breakify if needed
        if breakify:
            # load all relevant time breaks
            breaks = self.repo.list_time_breaks(
                    from_date=from_,
                    to_date=to)

            # Chop, chop!
            entries = breakify_entries(entries, breaks)

        return entries

    def get_breaks(self, from_: date, to: date) -> list[model.TimeBreak]:
        return self.repo.list_time_breaks(
                from_date=from_,
                to_date=to)

    def add_break(self, from_: datetime, to: datetime, comment: str):
        duration = timedelta(seconds=round((to - from_).total_seconds()))
        break_ = model.TimeBreak(start=from_, duration=duration, comment=comment)
        self.repo.insert_time_break(break_)
        return break_

    def add_missing_auto_breaks(self, date: date) -> list[config.AutoBreak]:
        auto_breaks = pydantic.parse_obj_as(list[config.AutoBreak], config.get("autoBreaks", []))
        time_entries = self.get_entries(date, date, breakify=True)
        ret = []
        for auto_break in auto_breaks:
            if config.Weekday.from_date(date) not in auto_break.weekdays:
                continue
            break_ = model.TimeBreak(
                    start=datetime.combine(datetime.now(), auto_break.start).astimezone(),
                    duration=(datetime.combine(datetime.now(), auto_break.stop) -
                              datetime.combine(datetime.now(), auto_break.start)),
                    comment=auto_break.comment)
            if any(entries_overlap(break_, e) for e in time_entries):
                self.add_break(break_.start, break_.start + break_.duration, break_.comment)
                ret.append(auto_break)
        return ret

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
                    is_open=False,
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
        local_entries = group_by(self.get_entries(from_, to, breakify=True),
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
                is_open=False,
                comment=comment))

        # Upsert it
        self.alvtime_client.upsert_time_entries(to_push)

        # Count it
        result.pushed_time_entries += len(to_push)

        return result

    def get_expected_durations(self, from_: date, to: date) -> dict[date, float]:
        ret = {}

        # Fetch bank holidays
        bank_holidays = self.alvtime_client.list_bank_holidays(from_.year, to.year)

        # Iterate over desired date range
        for current_date in iterate_dates(from_, to):
            expected_hours = timedelta(hours=7, minutes=30)

            # Is it weekend?
            if current_date.isoweekday() >= 6:
                expected_hours = timedelta()

            # Or is it a bank holiday?
            elif current_date in bank_holidays:
                expected_hours = timedelta()

            ret[current_date] = expected_hours

        return ret

    def get_total_registered_duration_by_date(self, from_: date, to: date, rounding: bool = False) -> dict[date, timedelta]:
        # Check if we're already started
        if self.current_entry():
            raise TaskAlreadyStartedError()

        ret = {}

        # Get all local entries, grouped by date
        local_entries = group_by(self.repo.list_time_entries(from_, to),
                                 lambda e: e.start.date())

        # Iterate over desired date range
        for current_date in iterate_dates(from_, to):
            # Find local hours
            entries = local_entries.get(current_date, [])
            duration = sum((e.duration for e in entries), timedelta())

            # Round if requestd
            if rounding:
                duration = self.round_duration(duration)

            ret[current_date] = duration

        return ret

    def check(self, from_: date, to: date) -> list[model.CheckResult]:
        ret = []
        expected_hours = self.get_expected_durations(from_, to)
        registered_hours = self.get_total_registered_duration_by_date(from_, to, rounding=True)

        # Iterate over desired date range
        for current_date in iterate_dates(from_, to):
            status = model.CheckResult(
                    date=current_date,
                    result_type=model.CheckResultType.ok,
                    registered_duration=registered_hours[current_date])

            # Check if enough hours has been registered
            if registered_hours[current_date] < expected_hours[current_date]:
                status.result_type = model.CheckResultType.warning
                expected = expected_hours[current_date].total_seconds() / 3600
                status.message = f"Not enough hours (expecting {expected:.1f})"

            ret.append(status)

        return ret

    def calculate_monetary_timebank(self, duration: timedelta) -> float:
        """
        Calculate monetary value of timebank hours based on configured salary.
        Returns unformatted float value (hourly rate * total hours).
        """
        salary: int = config.get(config.Keys.salary)
        hourly_rate = salary / 1950  # Assuming 1950 working hours per year
        return duration.total_seconds() / 60 / 60 * hourly_rate

    def get_total_hours_with_compensation(self, entries: list[model.AvailableHoursEntry]) -> float:
        """
        Calculate total hours multiplied by their compensation rates.
        Returns unformatted float value.
        """
        return sum(entry.hours * entry.compensation_rate for entry in entries)

    def calculate_unspent_overtime(self, entries: list[model.AvailableHoursEntry]) -> dict[str, float]:
        """
        Groups overtime entries by compensation rate.
        Returns dict with compensation rates as keys and total hours as values.

        Note: This includes ALL entries filtered by compensation rate,
        matching the frontend implementation in TimeBankOverview.vue
        """
        unspent_overtime: dict[str, float] = {
            "0.5": 0.0,   # Frivillig (Volunteer)
            "1.0": 0.0,   # Interntid (Mandatory)
            "1.5": 0.0,   # Fakturerbart (Billable)
            "2.0": 0.0,   # Tommy Time (Mandatory Billable)
        }

        for entry in entries:
            rate_key = f"{entry.compensation_rate}"
            if rate_key in unspent_overtime:
                unspent_overtime[rate_key] += entry.hours

        return unspent_overtime

    def get_available_hours(self) -> model.AvailableHours:
        return self.alvtime_client.get_available_hours()


    def order_payout(self, hours: float) -> model.GenericPayoutHourEntry:
        payout_hour_entry = model.GenericPayoutHourEntry(
            date=date.today(),
            hours=hours,
        )
        return self.alvtime_client.upsert_payout(payout_hour_entry)
