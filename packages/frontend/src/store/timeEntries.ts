import { State } from "./index";
import { ActionContext } from "vuex";
import { debounce } from "lodash";
import config from "@/config";
import httpClient from "../services/httpClient";

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

export interface TimeEntrieMap {
  [key: string]: TimeEntrieObj;
}

export interface TimeEntrieObj {
  value: string;
  id: number;
}

export interface ServerSideTimeEntrie {
  id: number;
  date: string;
  value: number;
  taskId: number;
  locked: boolean;
}

const state = {
  timeEntries: null,
  pushQueue: [],
  timeEntriesMap: {},
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
      try {
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
          })
          .catch(e => {
            if (e.status !== 200) {
              throw Error(`${e.statusText}
${e.data.title}`);
            }
            if (!Array.isArray(e.data) && e.data.message) {
              throw Error(e.data.message);
            }
          });
      } catch (e) {
        console.error(e);
        commit("ADD_TO_ERROR_LIST", e);
      }
    },
    1000
  ),

  FETCH_TIME_ENTRIES: async (
    { commit }: ActionContext<State, State>,
    params: { fromDateInclusive: string; toDateInclusive: string }
  ) => {
    const url = new URL(config.API_HOST + "/api/user/TimeEntries");
    url.search = new URLSearchParams(params).toString();

    await httpClient.get(url.toString()).then(response => {
      if (!Array.isArray(response.data) && response.data.message) {
        throw Error(response.data.message);
      }
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
