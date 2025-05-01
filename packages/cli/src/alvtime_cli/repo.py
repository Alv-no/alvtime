from threading import Lock
from contextlib import closing
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
                    to_time     TEXT,
                    task_id     INTEGER NOT NULL,
                    comment     TEXT)""")

    def _time_entry_from_dbo(self, dbo) -> model.TimeEntry:
        raise NotImplementedError()

    def _time_entry_to_dbo(self, time_entry) -> dict:
        raise NotImplementedError()

    def list_time_entries(self) -> list[model.TimeEntry]:
        with closing(self.db.cursor()) as cursor:
            sql = """
                SELECT * FROM time_entries
                """
            cursor.execute(sql)
            result = cursor.fetchall()

        return map(self._time_entry_from_dbo, result)

    def insert_time_entry(self, time_entry: model.TimeEntry) -> int:
        assert time_entry.id is None

        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                INSERT INTO time_entries
                """
            cursor.executemany(sql, (self._time_entry_to_dbo(time_entry), ))
            self.db.commit()
            return cursor.lastrowid
