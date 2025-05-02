from pydantic import BaseModel
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


class AlvtimeClient:
    def __init__(self, base_url: str = config.get(config.Keys.alvtime_base_url)):
        self.base_url = base_url

    def ping(self):
        response = requests.get(f"{self.base_url}/api/ping")
        return response.json()

    def list_tasks(self, include_locked: bool):
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}"}
        response = requests.get(f"{self.base_url}/api/user/Tasks",
                                headers=headers)
        response.raise_for_status()
        return list(map(_task_from_dto, response.json()))
