from pydantic import BaseModel
from datetime import datetime


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
    task_id: int
    name: str


class TimeEntry(BaseModel):
    id: int | None = None
    task_id: int
    start: datetime
    stop: datetime
    comment: str | None = None
