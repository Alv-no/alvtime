import { State } from "./index";
import { ActionContext } from "vuex";
import { debounce } from "lodash";
import config from "@/config";
import httpClient from "../services/httpClient";
import { Task } from "./tasks";

export interface TimeEntrieState {
  timeEntries: FrontendTimentrie[] | null;
  timeEntriesMap: TimeEntrieMap;
  pushQueue: FrontendTimentrie[];
}

export interface FrontendTimentrie {
  id: number;
  date: string;
  value: string;
  taskId: number;
  locked: boolean;
}

export interface EntriesSummarizedPerMonthPerTask {
  task: Task;
  summarizedHours: SummedHoursPrMonth[];
}

interface SummedHoursPrMonth {
  date: Date;
  value: number;
}

interface TimeEntrieMap {
  [key: string]: TimeEntrieObj;
}

interface TimeEntrieObj {
  value: string;
  id: number;
}

interface ServerSideTimeEntrie {
  id: number;
  date: string;
  value: number;
  taskId: number;
  locked: boolean;
}

interface TimeEntriesDateFormated {
  [key: string]: number | Date;
  id: number;
  value: number;
  date: Date;
  taskId: number;
}

const state = {
  timeEntries: null,
  pushQueue: [],
  timeEntriesMap: {},
};

const getters = {
  getTimeEntriesSummarizedPerMonthPerTask: (state: State) => {
    return monthSumPrTask(state.timeEntries, state.tasks);
  },

  getLastThreeMonthsForStatistics: () => {
    return getLastThreeMonths();
  },
};

const mutations = {
  UPDATE_TIME_ENTRIES(state: State, paramEntries: FrontendTimentrie[]) {
    updateTimeEntries(state, paramEntries);
  },
  UPDATE_TIME_ENTRIES_AFTER_UPDATE(
    state: State,
    paramEntries: FrontendTimentrie[]
  ) {
    updateTimeEntries(state, paramEntries);
  },
  ADD_TO_PUSH_QUEUE(state: State, paramEntrie: FrontendTimentrie) {
    state.pushQueue = updateArrayWith(state.pushQueue, paramEntrie);
  },

  FLUSH_PUSH_QUEUE(state: State) {
    state.pushQueue = [];
  },
};

const actions = {
  UPDATE_TIME_ENTRIE(
    { commit, dispatch }: ActionContext<State, State>,
    paramEntrie: FrontendTimentrie
  ) {
    commit("UPDATE_TIME_ENTRIES", [paramEntrie]);
    commit("ADD_TO_PUSH_QUEUE", paramEntrie);
    dispatch("DEBOUNCED_PUSH_TIME_ENTRIES");
  },

  DEBOUNCED_PUSH_TIME_ENTRIES: debounce(
    async ({ state, commit }: ActionContext<State, State>) => {
      const timeEntriesToPush = ([...state.pushQueue]
        .filter(entrie => isFloat(entrie.value))
        .map(createServerSideTimeEntrie)
        .filter(shouldBeStoredServerSide) as unknown) as ServerSideTimeEntrie[];

      if (!timeEntriesToPush.length) return;

      commit("FLUSH_PUSH_QUEUE");
      await httpClient
        .post<Array<Parameters<typeof createTimeEntrie>[0]>>(
          `${config.API_HOST}/api/user/TimeEntries`,
          timeEntriesToPush
        )
        .then(response => {
          commit(
            "UPDATE_TIME_ENTRIES_AFTER_UPDATE",
            response.data.map(createTimeEntrie)
          );
        });
    },
    1000
  ),

  FETCH_TIME_ENTRIES: (
    { commit }: ActionContext<State, State>,
    params: { fromDateInclusive: string; toDateInclusive: string }
  ) => {
    const url = new URL(config.API_HOST + "/api/user/TimeEntries");
    url.search = new URLSearchParams(params).toString();

    return httpClient.get(url.toString()).then(response => {
      const frontendTimeEntries = response.data
        .filter((entrie: ServerSideTimeEntrie) => entrie.value)
        .map(createTimeEntrie);
      commit("UPDATE_TIME_ENTRIES", frontendTimeEntries);
    });
  },
};

export default {
  state,
  mutations,
  actions,
  getters,
};

function updateTimeEntrieMap(
  timeEntrieMap: TimeEntrieMap,
  paramEntrie: FrontendTimentrie
): TimeEntrieMap {
  timeEntrieMap[`${paramEntrie.date}${paramEntrie.taskId}`] = {
    value: paramEntrie.value,
    id: paramEntrie.id,
  };
  return timeEntrieMap;
}

function updateArrayWith(
  arr: FrontendTimentrie[],
  paramEntrie: FrontendTimentrie
) {
  const index = arr.findIndex(entrie => isMatchingEntrie(paramEntrie, entrie));

  if (index !== -1) {
    return [
      ...arr.map(entrie =>
        isMatchingEntrie(paramEntrie, entrie) ? paramEntrie : entrie
      ),
    ];
  } else {
    return [...arr, paramEntrie];
  }
}

function isMatchingEntrie(
  entrieA: FrontendTimentrie,
  entrieB: FrontendTimentrie
) {
  return entrieA.date === entrieB.date && entrieA.taskId === entrieB.taskId;
}

function createTimeEntrie(data: any): FrontendTimentrie {
  return {
    ...data,
    date: data.date.split("T")[0],
    value: data.value.toString(),
  };
}

function createServerSideTimeEntrie(timeEntrie: FrontendTimentrie) {
  return {
    ...timeEntrie,
    value: Number(timeEntrie.value.replace(",", ".")),
  };
}

export function isFloat(str: string) {
  const commaMatches = str.match(/[.,]/g);
  const isMoreThanOneComma = commaMatches && commaMatches.length > 1;
  const isOnlyDigitsAndComma = !str.match(/[^0-9.,]/g);
  return isOnlyDigitsAndComma && !isMoreThanOneComma;
}

function shouldBeStoredServerSide(paramEntrie: ServerSideTimeEntrie) {
  return !isNonEntrieSetToZero(paramEntrie);
}

function isNonEntrieSetToZero(paramEntrie: ServerSideTimeEntrie) {
  return paramEntrie.value === 0 && paramEntrie.id === 0;
}

function updateTimeEntries(state: State, paramEntries: FrontendTimentrie[]) {
  let newTimeEntriesMap = { ...state.timeEntriesMap };
  for (const paramEntrie of paramEntries) {
    newTimeEntriesMap = updateTimeEntrieMap(newTimeEntriesMap, paramEntrie);
  }
  state.timeEntriesMap = { ...state.timeEntriesMap, ...newTimeEntriesMap };

  let newTimeEntries = state.timeEntries ? [...state.timeEntries] : [];
  for (const paramEntrie of paramEntries) {
    newTimeEntries = updateArrayWith(newTimeEntries, paramEntrie);
  }
  state.timeEntries = newTimeEntries;
}

function getLastThreeMonths(): Date[] {
  const today = new Date();
  const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1);
  const twoMonthsAgo = new Date(
    lastMonth.getFullYear(),
    lastMonth.getMonth() - 1
  );
  return [twoMonthsAgo, lastMonth, today];
}

function filterTimeEntriesByLastThreeMonths(
  allTimeEntriesAllTasks: TimeEntriesDateFormated[][]
) {
  const lastThreeMonths = getLastThreeMonths();
  function isInTheLastThreeMonths(date: Date): boolean {
    return lastThreeMonths.some(
      month =>
        month.getMonth() == date.getMonth() &&
        month.getFullYear() == date.getFullYear()
    );
  }

  const timeEntriesByMonths: TimeEntriesDateFormated[][] = [];
  for (let allTimeEntriesForOneTask of allTimeEntriesAllTasks) {
    const timeEntriesInLastThreeMonthsForOneTask = allTimeEntriesForOneTask.filter(
      ({ date }) => isInTheLastThreeMonths(date)
    );
    if (timeEntriesInLastThreeMonthsForOneTask.length > 0) {
      timeEntriesByMonths.push(timeEntriesInLastThreeMonthsForOneTask);
    }
  }
  return timeEntriesByMonths;
}

function convertStringToDateAndValueToNumber(timeEntries: FrontendTimentrie[]) {
  const mappedTimeEntries = timeEntries.map(entry => ({
    id: entry.id,
    value: Number(entry.value),
    taskId: entry.taskId,
    date: new Date(entry.date),
  }));

  return mappedTimeEntries;
}

function splitIntoTasks(
  timeEntriesFormated: TimeEntriesDateFormated[]
): TimeEntriesDateFormated[][] {
  const uniqueTaskIds = [
    ...new Set(timeEntriesFormated.map(({ taskId }) => taskId)),
  ];

  const timeEntriesSplitIntoTasks = [];
  for (let taskId of uniqueTaskIds) {
    const timeEntries = timeEntriesFormated.filter(
      timeEntrie => timeEntrie.taskId === taskId
    );
    timeEntriesSplitIntoTasks.push(timeEntries);
  }

  return timeEntriesSplitIntoTasks;
}

function monthSum(timeEntriesInLastThreeMonthsForSingleTask: TimeEntriesDateFormated[]) {
  const summedHoursPerMonth: SummedHoursPrMonth[] = getLastThreeMonths().map(
    month => {
      return {
        date: month,
        value: 0,
      };
    }
  );
  for (let timeEntrie of timeEntriesInLastThreeMonthsForSingleTask) {
    const monthOfEntry = summedHoursPerMonth.find(
      ({ date }) => date.getMonth() === timeEntrie.date.getMonth()
    );
    if (monthOfEntry) {
      monthOfEntry.value += timeEntrie.value;
    } else {
      summedHoursPerMonth.push({
        date: timeEntrie.date,
        value: timeEntrie.value,
      });
    }
  }
  return summedHoursPerMonth;
}

function monthSumPrTask(
  allTimeEntries: FrontendTimentrie[] | null,
  allTasks: Task[]
) {
  if (allTimeEntries) {
    const timeEntriesFormated = convertStringToDateAndValueToNumber(
      allTimeEntries
    );
    const timeEntriesSplitByTask = splitIntoTasks(timeEntriesFormated);
    const timeEntriesForLastThreeMonths = filterTimeEntriesByLastThreeMonths(
      timeEntriesSplitByTask
    );

    const entriesSummarizedPerMonthPerTask: EntriesSummarizedPerMonthPerTask[] = [];
    for (let timeEntriesLastThreeMonthsForOneTask of timeEntriesForLastThreeMonths) {
      if (timeEntriesLastThreeMonthsForOneTask.length > 0) {
        const taskLookup = allTasks.find(
          ({ id }) => id === timeEntriesLastThreeMonthsForOneTask[0].taskId
        );
        if (taskLookup === undefined) {
          throw new TypeError("Can not find task value");
        }
        entriesSummarizedPerMonthPerTask.push({
          task: taskLookup,
          summarizedHours: monthSum(timeEntriesLastThreeMonthsForOneTask),
        });
      }
    }
    return entriesSummarizedPerMonthPerTask;
  } else {
    return [];
  }
}
