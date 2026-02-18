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
        is_open=False,
        comment=dto["comment"])


def _time_entry_to_dto(entry: model.TimeEntry) -> dict:
    return {"taskId": entry.task_id,
            "date": entry.start.date().isoformat(),
            "value": entry.duration.total_seconds() / 3600.0,
            "comment": entry.comment}


def _payout_to_dto(entry: model.GenericPayoutHourEntry) -> dict:
    return {
        "date": entry.date.isoformat(),
        "hours": float(entry.hours)
    }

def _payout_from_dto(dto) -> model.GenericPayoutHourEntry:
    payout_date = dto.get("date")
    if payout_date is None:
        raise ValueError("Payout response missing date")

    try:
        parsed_date = date.fromisoformat(payout_date)
    except ValueError:
        parsed_date = datetime.fromisoformat(payout_date).date()

    return model.GenericPayoutHourEntry(
        date=parsed_date,
        hours=float(dto["hours"]))


class AlvtimeClient:
    def __init__(self, base_url: str | None = None):
        self.base_url = base_url or config.get(config.Keys.alvtime_base_url)

    def ping(self):
        headers = {"x-csrf": "dummy"}
        response = requests.get(f"{self.base_url}/api/ping", headers=headers)
        response.raise_for_status()
        return response.json()

    def list_tasks(self):
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}", "x-csrf": "dummy"}
        response = requests.get(f"{self.base_url}/api/user/Tasks",
                                headers=headers)
        response.raise_for_status()
        return list(map(_task_from_dto, response.json()))

    def list_time_entries(self, from_: date, to: date) -> list[model.TimeEntry]:
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}", "x-csrf": "dummy"}
        params = {"fromDateInclusive": from_.isoformat(),
                  "toDateInclusive": to.isoformat()}
        response = requests.get(f"{self.base_url}/api/user/TimeEntries",
                                headers=headers,
                                params=params)
        response.raise_for_status()
        return list(map(_time_entry_from_dto, response.json()))

    def upsert_time_entries(self, entries: list[model.TimeEntry]):
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}", "x-csrf": "dummy"}
        body = list(map(_time_entry_to_dto, entries))
        response = requests.post(f"{self.base_url}/api/user/TimeEntries",
                                 headers=headers,
                                 json=body)
        response.raise_for_status()
        return response.json()

    def list_bank_holidays(self, fromYear, toYear):
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}", "x-csrf": "dummy"}
        params = {"fromYearInclusive": str(fromYear),
                  "toYearInclusive": str(toYear)}
        response = requests.get(f"{self.base_url}/api/Holidays/Years",
                                headers=headers,
                                params=params)
        response.raise_for_status()

        return list(date.fromisoformat(dateString) for dateString in response.json())

    def get_available_hours(self) -> model.AvailableHours:
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}", "x-csrf": "dummy"} if pat else {}
        response = requests.get(f"{self.base_url}/api/user/AvailableHours",
                                headers=headers)
        response.raise_for_status()
        payload = response.json()
        if payload.get("entries") is None:
            payload["entries"] = []
        return model.AvailableHours.model_validate(payload)

    def upsert_payout(self, payout_hour_entry: model.GenericPayoutHourEntry) -> model.GenericPayoutHourEntry:
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}", "x-csrf": "dummy"} if pat else {}
        body = _payout_to_dto(payout_hour_entry)
        response = requests.post(
            f"{self.base_url}/api/user/Payouts",
            headers=headers,
            json=body)
        response.raise_for_status()
        return _payout_from_dto(response.json()) if response.content else None
