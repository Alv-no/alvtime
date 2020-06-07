import nodeFetch from "node-fetch";

interface Attributes {
  uri: string;
}

export interface RequestOptions {
  method?: string;
  headers?: { [key: string]: string };
  body?: string;
}

interface PrivateMethods {
  fetcher: (
    url: string,
    init?: RequestOptions
  ) => Promise<Task[] & TimeEntrie[] & ReportTimeEntrie[]>;
  getHeaders: (accessToken: string) => { headers: { [key: string]: string } };
  concatURL: (path: string, queryParams?: { [key: string]: string }) => string;
}

interface State extends Attributes, PrivateMethods {}

interface DateRange {
  [key: string]: string;
  fromDateInclusive: string;
  toDateInclusive: string;
}

export interface Task {
  id: number;
  name: string;
  description: string;
  hourRate: number;
  project: {
    id: number;
    name: string;
    customer: {
      id: number;
      name: string;
    };
  };
  favorite: boolean;
  locked: boolean;
}

export interface TimeEntrie {
  id: number;
  date: string;
  value: number;
  taskId: number;
}

export interface ReportTimeEntrie {
  user: number;
  userEmail: string;
  id: number;
  date: string;
  value: number;
  taskId: number;
}

export interface Client {
  getTasks: (accessToken: string) => Promise<Task[]>;
  editFavoriteTasks: (tasks: Task[], accessToken: string) => Promise<Task[]>;
  getTimeEntries: (
    dateRange: DateRange,
    accessToken: string
  ) => Promise<TimeEntrie[]>;
  editTimeEntries: (
    timeEntries: TimeEntrie[],
    accessToken: string
  ) => Promise<TimeEntrie[]>;
  getTimeEntriesReport: (
    dateRange: DateRange,
    accessToken: string
  ) => Promise<ReportTimeEntrie[]>;
}

type FetchFunc = (
  uri: string,
  init: RequestOptions
) => Promise<{ json(): Promise<any>; status: number; statusText: string }>;

export default function createAlvtimeClient(
  uri: string,
  fetch: FetchFunc = nodeFetch
): Client {
  async function fetcher(url: string, init: RequestOptions) {
    const response = await fetch(url, init);
    if (response.status !== 200) {
      throw response.statusText;
    }
    return response.json();
  }

  function getHeaders(accessToken: string) {
    return {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
    };
  }

  function concatURL(path: string, queryParams?: { [key: string]: string }) {
    const url = new URL(uri + path);
    if (queryParams) url.search = new URLSearchParams(queryParams).toString();
    return url.toString();
  }

  return {
    getTasks(accessToken: string) {
      return fetcher(concatURL("/api/user/tasks"), {
        ...getHeaders(accessToken),
      });
    },

    editFavoriteTasks(tasks: Task[], accessToken: string) {
      const method = "post";
      const body = JSON.stringify(tasks);
      const init = { method, ...getHeaders(accessToken), body };
      return fetcher(concatURL("/api/user/Tasks"), init);
    },

    getTimeEntries(dateRange: DateRange, accessToken: string) {
      return fetcher(concatURL("/api/user/TimeEntries", dateRange), {
        ...getHeaders(accessToken),
      });
    },

    editTimeEntries(timeEntries: TimeEntrie[], accessToken: string) {
      const method = "post";
      const body = JSON.stringify(timeEntries);
      const init = { method, ...getHeaders(accessToken), body };
      return fetcher(concatURL("/api/user/TimeEntries"), init);
    },

    getTimeEntriesReport(dateRange: DateRange, accessToken: string) {
      return fetcher(concatURL("/api/user/TimeEntriesReport", dateRange), {
        ...getHeaders(accessToken),
      });
    },
  };
}
