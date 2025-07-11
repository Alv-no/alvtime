from pydantic import BaseModel
from datetime import datetime, timedelta


class Customer(BaseModel):
    id: int
    name: str


class Project(BaseModel):
    id: int
    name: str
    customer_id: int


class Task(BaseModel):
    id: int
    name: str
    locked: bool
    rate: float
    project_name: str
    customer_name: str


class TaskAlias(BaseModel):
    name: str
    task: Task


class TimeEntry(BaseModel):
    id: int | None = None
    task_id: int
    start: datetime | None = None
    duration: timedelta | None = None
    comment: str | None = None
    is_changed: bool = False
    task: Task | None = None
