from threading import Lock
from contextlib import closing
from datetime import datetime, timedelta
import sqlite3
from . import model


class Repo:
    def __init__(self, db_filename):
        self.write_lock = Lock()
        self.db = sqlite3.connect(db_filename, check_same_thread=False)
        self.db.row_factory = sqlite3.Row

        self._create_schema()

    def _create_schema(self):
        with self.write_lock, closing(self.db.cursor()) as cursor:
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS time_entries (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    alvtime_id  INTEGER,
                    from_time   TEXT,
                    duration    INTEGER,
                    task_id     INTEGER NOT NULL,
                    comment     TEXT,
                    is_changed  INTEGER NOT NULL)""")

    def _time_entry_from_dbo(self, dbo) -> model.TimeEntry:
        return model.TimeEntry(
            id=dbo["id"],
            alvtime_id=dbo["alvtime_id"],
            task_id=dbo["task_id"],
            start=datetime.fromisoformat(dbo["from_time"]),
            duration=timedelta(seconds=dbo["duration"]) if dbo["duration"] else None,
            comment=dbo["comment"],
            is_changed=(dbo["is_changed"] != 0)
        )

    def _time_entry_to_dbo(self, time_entry: model.TimeEntry) -> dict:
        return {
            "id": time_entry.id,
            "alvtime_id": time_entry.alvtime_id,
            "from_time": time_entry.start.isoformat(),
            "duration": int(time_entry.duration.total_seconds()) if time_entry.duration else None,
            "task_id": time_entry.task_id,
            "comment": time_entry.comment,
            "is_changed": time_entry.is_changed
        }

    def list_time_entries(self) -> list[model.TimeEntry]:
        with closing(self.db.cursor()) as cursor:
            sql = """
                SELECT * FROM time_entries
                """
            cursor.execute(sql)
            result = cursor.fetchall()

        return map(self._time_entry_from_dbo, result)

    def insert_time_entry(self, time_entry: model.TimeEntry) -> int:
        if time_entry.id is not None:
            raise ValueError("'id' should be None. Trying to insert existing item?")

        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                INSERT INTO time_entries (
                    id,
                    alvtime_id,
                    from_time,
                    duration,
                    task_id,
                    comment,
                    is_changed)
                VALUES (
                    :id,
                    NULL,
                    :from_time,
                    :duration,
                    :task_id,
                    :comment,
                    :is_changed)
                """
            cursor.executemany(sql, (self._time_entry_to_dbo(time_entry), ))
            self.db.commit()
            return cursor.lastrowid

    def update_time_entry(self, time_entry: model.TimeEntry) -> model.TimeEntry:
        if time_entry.id is None:
            raise ValueError("'id' should not be be None. Trying to update non-existing item?")

        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                UPDATE time_entries SET
                    alvtime_id = :alvtime_id,
                    from_time = :from_time,
                    duration = :duration,
                    task_id = :task_id,
                    comment = :comment,
                    is_changed = :is_changed
                WHERE
                    id = :id
                """
            cursor.executemany(sql, (self._time_entry_to_dbo(time_entry), ))
            self.db.commit()
        return time_entry

    def find_open_entries(self) -> list[model.TimeEntry]:
        with closing(self.db.cursor()) as cursor:
            sql = """
                SELECT * FROM time_entries
                WHERE duration IS NULL
                """
            cursor.execute(sql)
            return list(map(self._time_entry_from_dbo, cursor.fetchall()))
