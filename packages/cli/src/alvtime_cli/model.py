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
    project_id: int


class TaskAlias(BaseModel):
    id: str
    task_id: int


class TimeEntry(BaseModel):
    id: int | None = None
    task_id: int
    start: datetime
    stop: datetime
    comment: str | None = None
