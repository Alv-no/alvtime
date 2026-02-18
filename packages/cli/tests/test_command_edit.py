from datetime import date, datetime, timedelta
from unittest.mock import MagicMock, call
from alvtime_cli.local_service import LocalService
from alvtime_cli.commands.edit import _perform_changes, EditModel, EditEntry, EditBreak
from alvtime_cli import model


def test_perform_changes_add_entry():
    svc = MagicMock(spec=LocalService)

    orig = EditModel(
            entries=[
                EditEntry(ref=1, task_id=1, task="foo", date=date(2020, 1, 1), start="10:00", stop="12:00")
                ],
            breaks=[
                EditBreak(ref=1, date=date(2020, 1, 1), start="12:00", stop="13:00")])

    response = EditModel(
            entries=[
                EditEntry(ref=1, task_id=1, task="foo", date=date(2020, 1, 1),  start="10:00", stop="12:00"),
                EditEntry(task_id=1, task="foo", date=date(2020, 1, 1),  start="10:00", stop="12:00"),
                ],
            breaks=[
                EditBreak(ref=1, date=date(2020, 1, 1), start="12:00", stop="13:00")
                ])

    _perform_changes(svc, orig, response)

    assert svc.mock_calls == [
        call.add_time_entry(
                model.TimeEntry(
                    start=datetime(2020, 1, 1, 10, 0).astimezone(),
                    duration=timedelta(hours=2),
                    task_id=1,
                    is_open=False))
    ]


def test_perform_changes_add_break():
    svc = MagicMock(spec=LocalService)

    orig = EditModel(
            entries=[
                EditEntry(ref=1, task_id=1, task="foo", date=date(2020, 1, 1), start="10:00", stop="12:00")
                ],
            breaks=[
                EditBreak(ref=1, date=date(2020, 1, 1), start="12:00", stop="13:00")])

    response = EditModel(
            entries=[
                EditEntry(ref=1, task_id=1, task="foo", date=date(2020, 1, 1),  start="10:00", stop="12:00"),
                ],
            breaks=[
                EditBreak(ref=1, date=date(2020, 1, 1), start="12:00", stop="13:00"),
                EditBreak(date=date(2020, 1, 1), start="12:00", stop="14:00", comment="Foo"),
                ])

    _perform_changes(svc, orig, response)

    assert svc.mock_calls == [
        call.add_break(
                model.TimeBreak(
                    start=datetime(2020, 1, 1, 12, 0).astimezone(),
                    duration=timedelta(hours=2),
                    comment="Foo"))
    ]
