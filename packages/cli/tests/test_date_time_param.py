from datetime import datetime
from alvtime_cli.param_types import DateTimeParam


def test_yyyy_mm_dd_hh_mm_ss():
    truth = datetime(
        year=2020,
        month=3,
        day=13,
        hour=9,
        minute=10,
        second=11,
        microsecond=0
    ).astimezone()
    assert DateTimeParam("2020-03-13 09:10:11").isoformat() == truth.isoformat()


def test_yyyy_mm_dd_hh_mm():
    truth = datetime(
        year=2020,
        month=3,
        day=13,
        hour=9,
        minute=10,
        second=00,
        microsecond=0
    ).astimezone()
    assert DateTimeParam("2020-03-13 09:10").isoformat() == truth.isoformat()


def test_hh_mm():
    truth = datetime.today().replace(
        hour=14,
        minute=15,
        second=0,
        microsecond=0
    ).astimezone()
    assert DateTimeParam("14:15").isoformat() == truth.isoformat()


def test_hh_mm_ss():
    truth = datetime.today().replace(
        hour=9,
        minute=10,
        second=11,
        microsecond=0
    ).astimezone()
    assert DateTimeParam("09:10:11").isoformat() == truth.isoformat()
