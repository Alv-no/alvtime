from datetime import date, datetime, timedelta
import requests
from . import config
from . import model


def _task_from_dto(dto) -> model.Task:
    return model.Task(
        id=dto["id"],
        name=dto["name"],
        locked=dto["locked"],
        rate=dto["compensationRate"],
        project_name=dto["project"]["name"],
        customer_name=dto["project"]["customer"]["name"])


def _time_entry_from_dto(dto) -> model.TimeEntry:
    return model.TimeEntry(
        task_id=dto["taskId"],
        start=datetime.fromisoformat(dto["date"]),
        duration=timedelta(hours=dto["value"]),
        comment=dto["comment"])


def _time_entry_to_dto(entry: model.TimeEntry) -> dict:
    return {"taskId": entry.task_id,
            "date": entry.start.date().isoformat(),
            "value": entry.duration.total_seconds() / 3600.0,
            "comment": entry.comment}


class AlvtimeClient:
    def __init__(self, base_url: str | None = None):
        self.base_url = base_url or config.get(config.Keys.alvtime_base_url)

    def ping(self):
        response = requests.get(f"{self.base_url}/api/ping")
        return response.json()

    def list_tasks(self):
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}"}
        response = requests.get(f"{self.base_url}/api/user/Tasks",
                                headers=headers)
        response.raise_for_status()
        return list(map(_task_from_dto, response.json()))

    def list_time_entries(self, from_: date, to: date) -> list[model.TimeEntry]:
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}"}
        params = {"fromDateInclusive": from_.isoformat(),
                  "toDateInclusive": to.isoformat()}
        response = requests.get(f"{self.base_url}/api/user/TimeEntries",
                                headers=headers,
                                params=params)
        response.raise_for_status()
        return list(map(_time_entry_from_dto, response.json()))

    def upsert_time_entries(self, entries: list[model.TimeEntry]):
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}"}
        body = list(map(_time_entry_to_dto, entries))
        response = requests.post(f"{self.base_url}/api/user/TimeEntries",
                                 headers=headers,
                                 json=body)
        response.raise_for_status()
        return response.json()
