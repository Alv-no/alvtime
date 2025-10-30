from __future__ import annotations

from datetime import date as DateType, datetime, timedelta
from enum import Enum, IntEnum

from pydantic import BaseModel, ConfigDict, Field


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


class BaseEntry(BaseModel):
    id: int | None = None
    start: datetime
    duration: timedelta
    comment: str


class TimeEntry(BaseEntry):
    id: int | None = None
    task_id: int
    start: datetime | None = None
    duration: timedelta | None = None
    is_open: bool
    comment: str | None = None
    is_changed: bool = False
    task: Task | None = None


class CheckResultType(str, Enum):
    ok = "ok"
    warning = "warning"
    error = "error"


class CheckResult(BaseModel):
    date: date
    result_type: CheckResultType
    registered_duration: timedelta
    message: str = ""


class TimeBreak(BaseEntry):
    pass


class TimebankEntryType(IntEnum):
    OVERTIME = 0
    PAYOUT = 1
    FLEX = 2


class AvailableHoursEntry(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    date: DateType | None = None
    hours: float
    compensation_rate: float = Field(alias="compensationRate")
    type: TimebankEntryType
    active: bool | None = None


class AvailableHours(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    available_hours_before_compensation: float = Field(alias="availableHoursBeforeCompensation")
    available_hours_after_compensation: float = Field(alias="availableHoursAfterCompensation")
    entries: list[AvailableHoursEntry] = Field(default_factory=list)
