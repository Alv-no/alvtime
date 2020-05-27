import fetch from "node-fetch";

interface Attributes {
  uri: string;
}

interface RequestOptions {
  method?: string;
  headers?: { [key: string]: string };
  body?: string;
}

interface PrivateMethods {
  fetcher: (
    url: string,
    init?: RequestOptions
  ) => Promise<Task[] & TimeEntrie[]>;
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
}

export default function createAlvtimeClient(uri: string): Client {
  const attributes = {
    uri,
  };

  const privateMethods = {
    ...createFetcher(attributes),
    ...createHeadersGetter(attributes),
    ...createURLConcatter(attributes),
  };

  const state = {
    ...attributes,
    ...privateMethods,
  };

  return {
    ...createGetTasks(state),
    ...createEditFavoriteTasks(state),
    ...createGetTimeEntries(state),
    ...createEditTimeEntries(state),
  };
}

function createGetTasks({ getHeaders, fetcher, concatURL }: State) {
  const getTasks = async (accessToken: string) => {
    return fetcher(concatURL("/api/user/tasks"), {
      ...getHeaders(accessToken),
    });
  };
  return { getTasks };
}

function createEditFavoriteTasks({ fetcher, getHeaders, concatURL }: State) {
  const editFavoriteTasks = async (tasks: Task[], accessToken: string) => {
    const method = "post";
    const body = JSON.stringify(tasks);
    const init = { method, ...getHeaders(accessToken), body };
    return fetcher(concatURL("/api/user/Tasks"), init);
  };
  return { editFavoriteTasks };
}

function createGetTimeEntries({ fetcher, getHeaders, concatURL }: State) {
  const getTimeEntries = async (dateRange: DateRange, accessToken: string) => {
    return fetcher(concatURL("/api/user/TimeEntries", dateRange), {
      ...getHeaders(accessToken),
    });
  };
  return { getTimeEntries };
}

function createEditTimeEntries({ fetcher, getHeaders, concatURL }: State) {
  const editTimeEntries = async (
    timeEntries: TimeEntrie[],
    accessToken: string
  ) => {
    const method = "post";
    const body = JSON.stringify(timeEntries);
    const init = { method, ...getHeaders(accessToken), body };
    return fetcher(concatURL("/api/user/TimeEntries"), init);
  };
  return { editTimeEntries };
}

function createFetcher(attributes: Attributes) {
  const fetcher = async (url: string, init: RequestOptions) => {
    const response = await fetch(url, init);
    if (response.status !== 200) {
      throw response.statusText;
    }
    return response.json();
  };

  return { fetcher };
}

function createHeadersGetter(attributes: Attributes) {
  const getHeaders = (accessToken: string) => {
    return {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
    };
  };
  return { getHeaders };
}

function createURLConcatter(attributes: Attributes) {
  const concatURL = (path: string, queryParams?: { [key: string]: string }) => {
    const url = new URL(attributes.uri + path);
    if (queryParams) url.search = new URLSearchParams(queryParams).toString();
    return url.toString();
  };
  return { concatURL };
}
