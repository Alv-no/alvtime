from threading import Lock
from contextlib import closing
from datetime import date, datetime, timedelta
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
                    from_time   TEXT,
                    duration    INTEGER,
                    task_id     INTEGER NOT NULL,
                    comment     TEXT,
                    is_changed  INTEGER NOT NULL)""")
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS time_breaks (
                    id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    from_time   TEXT,
                    duration    INTEGER,
                    comment     TEXT)""")
            cursor.execute("""
                CREATE TABLE IF NOT EXISTS tasks (
                    id            INTEGER PRIMARY KEY,
                    name          TEXT NOT NULL,
                    project_name  TEXT NOT NULL,
                    customer_name TEXT NOT NULL,
                    rate          REAL NOT NULL,
                    locked        INTEGER NOT NULL)""")

    def _time_entry_from_dbo(self, dbo) -> model.TimeEntry:
        return model.TimeEntry(
            id=dbo["id"],
            task_id=dbo["task_id"],
            start=datetime.fromisoformat(dbo["from_time"]),
            duration=timedelta(seconds=dbo["duration"]) if dbo["duration"] else None,
            comment=dbo["comment"],
            is_changed=(dbo["is_changed"] != 0)
        )

    def _time_entry_to_dbo(self, time_entry: model.TimeEntry) -> dict:
        return {
            "id": time_entry.id,
            "from_time": time_entry.start.isoformat(),
            "duration": int(time_entry.duration.total_seconds()) if time_entry.duration else None,
            "task_id": time_entry.task_id,
            "comment": time_entry.comment,
            "is_changed": time_entry.is_changed
        }

    def _time_break_from_dbo(self, dbo) -> model.TimeBreak:
        return model.TimeBreak(
            id=dbo["id"],
            start=datetime.fromisoformat(dbo["from_time"]),
            duration=timedelta(seconds=dbo["duration"]),
            comment=dbo["comment"]
        )

    def _time_break_to_dbo(self, time_break: model.TimeBreak) -> dict:
        return {
            "id": time_break.id,
            "from_time": time_break.start.isoformat(),
            "duration": int(time_break.duration.total_seconds()),
            "comment": time_break.comment
        }

    def _task_from_dbo(self, dbo) -> model.Task:
        return model.Task(
            id=dbo["id"],
            name=dbo["name"],
            project_name=dbo["project_name"],
            customer_name=dbo["customer_name"],
            rate=dbo["rate"],
            locked=(dbo["locked"] != 0)
        )

    def _task_to_dbo(self, task: model.Task) -> dict:
        return {
            "id": task.id,
            "name": task.name,
            "project_name": task.project_name,
            "customer_name": task.customer_name,
            "rate": task.rate,
            "locked": task.locked
        }

    def list_time_entries(self, from_date: date = None,
                          to_date: date = None) -> list[model.TimeEntry]:
        with closing(self.db.cursor()) as cursor:
            sql = "SELECT * FROM time_entries"
            where = []
            params = {}

            if from_date:
                where.append("from_time >= :from_date")
                params["from_date"] = from_date.isoformat()
            if to_date:
                where.append("from_time < :to_date")
                params["to_date"] = (to_date + timedelta(days=1)).isoformat()
            if where:
                sql = sql + " WHERE " + " AND ".join(where)
            cursor.execute(sql, params)
            result = cursor.fetchall()

        return list(map(self._time_entry_from_dbo, result))

    def get_last_time_entry(self) -> model.TimeEntry:
        with closing(self.db.cursor()) as cursor:
            sql = """
                SELECT * FROM time_entries ORDER BY ROWID DESC LIMIT 1
                """
            cursor.execute(sql)
            dbo = cursor.fetchone()
            if not dbo:
                return None
            return self._time_entry_from_dbo(dbo)

    def insert_time_entry(self, time_entry: model.TimeEntry) -> int:
        if time_entry.id is not None:
            raise ValueError("'id' should be None. Trying to insert existing item?")

        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                INSERT INTO time_entries (
                    id,
                    from_time,
                    duration,
                    task_id,
                    comment,
                    is_changed)
                VALUES (
                    :id,
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

    def delete_time_entry(self, time_entry_id: int):
        if time_entry_id is None:
            raise ValueError("'id' should not be be None. Trying to update non-existing item?")

        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                DELETE FROM time_entries
                WHERE
                    id = :id
                """
            cursor.executemany(sql, ({"id": time_entry_id}, ))
            self.db.commit()

    def find_open_entries(self) -> list[model.TimeEntry]:
        with closing(self.db.cursor()) as cursor:
            sql = """
                SELECT * FROM time_entries
                WHERE duration IS NULL
                """
            cursor.execute(sql)
            return list(map(self._time_entry_from_dbo, cursor.fetchall()))

    def list_time_breaks(self, from_date: date = None,
                         to_date: date = None) -> list[model.TimeBreak]:
        with closing(self.db.cursor()) as cursor:
            sql = "SELECT * FROM time_breaks"
            where = []
            params = {}

            if from_date:
                where.append("from_time >= :from_date")
                params["from_date"] = from_date.isoformat()
            if to_date:
                where.append("from_time < :to_date")
                params["to_date"] = (to_date + timedelta(days=1)).isoformat()
            if where:
                sql = sql + " WHERE " + " AND ".join(where)
            cursor.execute(sql, params)
            result = cursor.fetchall()

        return list(map(self._time_break_from_dbo, result))

    def insert_time_break(self, time_break: model.TimeBreak) -> int:
        if time_break.id is not None:
            raise ValueError("'id' should be None. Trying to insert existing item?")

        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                INSERT INTO time_breaks (
                    id,
                    from_time,
                    duration,
                    comment)
                VALUES (
                    :id,
                    :from_time,
                    :duration,
                    :comment)
                """
            cursor.executemany(sql, (self._time_break_to_dbo(time_break), ))
            self.db.commit()
            return cursor.lastrowid

    def update_time_break(self, time_break: model.TimeBreak) -> model.TimeBreak:
        if time_break.id is None:
            raise ValueError("'id' should not be be None. Trying to update non-existing item?")

        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                UPDATE time_breaks SET
                    from_time = :from_time,
                    duration = :duration,
                    comment = :comment
                WHERE
                    id = :id
                """
            cursor.executemany(sql, (self._time_break_to_dbo(time_break), ))
            self.db.commit()
        return time_break

    def delete_time_break(self, time_break_id: int):
        if time_break_id is None:
            raise ValueError("'id' should not be be None. Trying to delete non-existing item?")

        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                DELETE FROM time_breaks
                WHERE
                    id = :id
                """
            cursor.executemany(sql, ({"id": time_break_id}, ))
            self.db.commit()

    def list_tasks(self) -> list[model.Task]:
        with closing(self.db.cursor()) as cursor:
            sql = """
                SELECT * FROM tasks
                """
            cursor.execute(sql)
            result = cursor.fetchall()

        return list(map(self._task_from_dbo, result))

    def find_task(self, task_id: int) -> model.Task:
        with closing(self.db.cursor()) as cursor:
            sql = """
                SELECT * FROM tasks
                WHERE
                    id = :id
                """
            cursor.execute(sql, {"id": task_id})
            return self._task_from_dbo(cursor.fetchone())

    def upsert_task(self, task: model.Task) -> model.Task:
        with self.write_lock, closing(self.db.cursor()) as cursor:
            sql = """
                INSERT INTO tasks (
                    id,
                    name,
                    project_name,
                    customer_name,
                    rate,
                    locked)
                VALUES (
                    :id,
                    :name,
                    :project_name,
                    :customer_name,
                    :rate,
                    :locked)
                ON CONFLICT(id) DO
                UPDATE SET
                    name = :name,
                    project_name = :project_name,
                    customer_name = :customer_name,
                    rate = :rate,
                    locked = :locked
                """
            cursor.executemany(sql, (self._task_to_dbo(task), ))
            self.db.commit()
        return task
