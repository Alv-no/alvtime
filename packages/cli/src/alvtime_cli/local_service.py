from .repo import Repo
from .alvtime_client import AlvtimeClient


class LocalService:
    def __init__(self, repo: Repo, alvtime_client: AlvtimeClient):
        self.repo = repo
        self.alvtime_client = alvtime_client
