from pydantic import BaseModel
import requests
from . import config


class Task(BaseModel):
    id: int
    name: str
    locked: bool
    rate: float


class Project(BaseModel):
    id: int
    name: str
    tasks: list[Task]


class Customer(BaseModel):
    id: int
    name: str
    projects: list[Project]


def _task_from_dto(dto) -> Task:
    return Task(
        id=dto["id"],
        name=dto["name"],
        locked=dto["locked"],
        rate=dto["compensationRate"]
    )


def _project_from_dto(dto) -> Project:
    return Project(
        id=dto["id"],
        name=dto["name"],
        tasks=[])


def _customer_from_dto(dto) -> Customer:
    return Customer(
        id=dto["id"],
        name=dto["name"],
        projects=[])


class AlvtimeClient:
    def __init__(self, base_url: str = "https://api.alvtime.no"):
        self.base_url = base_url

    def ping(self):
        response = requests.get(f"{self.base_url}/api/ping")
        return response.json()

    def list_tasks(self):
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}"}
        response = requests.get(f"{self.base_url}/api/user/Tasks",
                                headers=headers)
        response.raise_for_status()
        return response.json()

    def list_customers(self, include_locked):
        pat = config.get(config.Keys.personal_access_token, "")
        headers = {"Authorization": f"Bearer {pat}"}
        response = requests.get(f"{self.base_url}/api/user/Tasks",
                                headers=headers)
        response.raise_for_status()
        data = response.json()

        customers: list[Customer] = []

        for taskDto in data:
            customer_id = taskDto["project"]["customer"]["id"]
            customer = next((c for c in customers if c.id == customer_id), None)
            if not customer:
                customer = _customer_from_dto(taskDto["project"]["customer"])
                customers.append(customer)

            project_id = taskDto["project"]["id"]
            project = next((p for p in customer.projects if p.id == project_id), None)
            if not project:
                project = _project_from_dto(taskDto["project"])
                customer.projects.append(project)

            task_id = taskDto["id"]
            task = next((t for t in project.tasks if t.id == task_id), None)
            if not task:
                task = _task_from_dto(taskDto)
                if include_locked or not task.locked:
                    project.tasks.append(task)

        # Prune empty projects
        for customer in customers:
            customer.projects = [p for p in customer.projects if len(p.tasks) > 0]

        # Prune empty customers
        customers = [c for c in customers if len(c.projects) > 0]

        return customers
