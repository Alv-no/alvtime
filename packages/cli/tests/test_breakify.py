from datetime import datetime, timedelta
from alvtime_cli import model, utils


def test_no_overlap_before():
    entry = model.TimeEntry(
        start=datetime(2025, 1, 1, 8, 0, 0).astimezone(),
        duration=timedelta(minutes=8*60),
        comment="",
        task_id=0)
    brk1 = model.TimeBreak(
        start=datetime(2025, 1, 1, 7, 0, 0).astimezone(),
        duration=timedelta(minutes=60),
        comment="")
    entries = utils.breakify_entries([entry.model_copy()], [brk1])

    assert len(entries) == 1
    assert entries[0] == entry


def test_no_overlap_after():
    entry = model.TimeEntry(
        start=datetime(2025, 1, 1, 8, 0, 0).astimezone(),
        duration=timedelta(minutes=8*60),
        comment="",
        task_id=0)
    brk1 = model.TimeBreak(
        start=datetime(2025, 1, 1, 16, 0, 0).astimezone(),
        duration=timedelta(minutes=5),
        comment="")
    entries = utils.breakify_entries([entry.model_copy()], [brk1])

    assert len(entries) == 1
    assert entries[0] == entry


def test_overlap_start():
    entry = model.TimeEntry(
        start=datetime(2025, 1, 1, 8, 0, 0).astimezone(),
        duration=timedelta(minutes=8*60),
        comment="",
        task_id=0)
    brk1 = model.TimeBreak(
        start=datetime(2025, 1, 1, 7, 0, 0).astimezone(),
        duration=timedelta(minutes=61, seconds=30),
        comment="")
    entries = utils.breakify_entries([entry.model_copy()], [brk1])

    assert len(entries) == 1
    assert entries[0].start.strftime("%Y-%m-%d %H:%M:%S") == "2025-01-01 08:01:30"
    assert entries[0].duration == timedelta(hours=7, minutes=58, seconds=30)


def test_overlap_end():
    entry = model.TimeEntry(
        start=datetime(2025, 1, 1, 8, 0, 0).astimezone(),
        duration=timedelta(minutes=8*60),
        comment="",
        task_id=0)
    brk1 = model.TimeBreak(
        start=datetime(2025, 1, 1, 15, 40, 11).astimezone(),
        duration=timedelta(minutes=60),
        comment="")
    entries = utils.breakify_entries([entry.model_copy()], [brk1])

    assert len(entries) == 1
    assert entries[0].start.strftime("%Y-%m-%d %H:%M:%S") == "2025-01-01 08:00:00"
    assert entries[0].duration == timedelta(hours=7, minutes=40, seconds=11)
    entries[0].start = entry.start
    entries[0].duration = entry.duration
    assert entries[0] == entry


def test_overlap_middle():
    entry = model.TimeEntry(
        start=datetime(2025, 1, 1, 8, 0, 0).astimezone(),
        duration=timedelta(minutes=8*60),
        comment="",
        task_id=0)
    brk1 = model.TimeBreak(
        start=datetime(2025, 1, 1, 12, 30, 0).astimezone(),
        duration=timedelta(minutes=60),
        comment="")
    entries = utils.breakify_entries([entry.model_copy()], [brk1])

    assert len(entries) == 2
    assert entries[0].start.strftime("%Y-%m-%d %H:%M:%S") == "2025-01-01 08:00:00"
    assert entries[0].duration == timedelta(hours=4, minutes=30)
    entries[0].start = entry.start
    entries[0].duration = entry.duration
    assert entries[0] == entry

    assert entries[1].start.strftime("%Y-%m-%d %H:%M:%S") == "2025-01-01 13:30:00"
    assert entries[1].duration == timedelta(hours=2, minutes=30)
    entries[1].start = entry.start
    entries[1].duration = entry.duration
    assert entries[1] == entry
