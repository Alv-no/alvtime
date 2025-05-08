from threading import Lock
from contextlib import closing
from datetime import datetime
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
            task_id=dbo["task_id"],
            start=datetime.fromisoformat(dbo["from_time"]),
            duration=dbo["duration"],
            comment=dbo["comment"]
        )

    def _time_entry_to_dbo(self, time_entry: model.TimeEntry) -> dict:
        return {
            "id": time_entry.id,
            "from_time": time_entry.start.isoformat(),
            "duration": time_entry.duration.total_seconds if time_entry.duration else None,
            "task_id": time_entry.task_id,
            "comment": time_entry.comment,
            "is_changed": 1
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

    def find_open_entries(self) -> list[model.TimeEntry]:
        with closing(self.db.cursor()) as cursor:
            sql = """
                SELECT * FROM time_entries
                WHERE duration IS NULL
                """
            cursor.execute(sql)
            return list(map(self._time_entry_from_dbo, cursor.fetchall()))
