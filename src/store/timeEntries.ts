import moment from "moment";
import { State, FrontendTimentrie } from "./index";
import { ActionContext } from "vuex";
import { debounce } from "lodash";
import config from "@/config";
import { adAuthenticatedFetch } from "@/services/auth";

export interface ServerSideTimeEntrie {
  id: number;
  date: string;
  value: number;
  taskId: number;
}

export default {
  state: {
    timeEntries: [],
    pushQueue: [],
  },

  getters: {
    getTimeEntrie: (state: State) => (entrieA: FrontendTimentrie) => {
      return state.timeEntries.find((entrieB: FrontendTimentrie) =>
        isMatchingEntrie(entrieA, entrieB)
      );
    },
  },

  mutations: {
    SET_TIME_ENTRIES(state: State, paramEntries: FrontendTimentrie[]) {
      state.timeEntries = paramEntries;
    },

    UPDATE_TIME_ENTRIES(state: State, paramEntries: FrontendTimentrie[]) {
      for (const paramEntrie of paramEntries) {
        state.timeEntries = updateArrayWith(state.timeEntries, paramEntrie);
      }
    },

    ADD_TO_PUSH_QUEUE(state: State, paramEntrie: FrontendTimentrie) {
      state.pushQueue = updateArrayWith(state.pushQueue, paramEntrie);
    },

    FLUSH_PUSH_QUEUE(state: State) {
      state.pushQueue = [];
    },
  },

  actions: {
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
          .filter(
            shouldBeStoredServerSide
          ) as unknown) as ServerSideTimeEntrie[];

        if (!timeEntriesToPush.length) return;

        commit("FLUSH_PUSH_QUEUE");
        try {
          const response = await adAuthenticatedFetch(
            config.HOST + "/api/user/TimeEntries",
            {
              method: "post",
              headers: {
                "Content-Type": "application/json",
              },
              body: JSON.stringify(timeEntriesToPush),
            }
          );
          const timeEntries = await response.json();
          if (response.status !== 200) {
            throw Error(`${response.statusText}
${timeEntries.title}`);
          }
          if (!Array.isArray(timeEntries) && timeEntries.message) {
            throw Error(timeEntries.message);
          }
          if (Array.isArray(timeEntries)) {
            commit("UPDATE_TIME_ENTRIES", timeEntries.map(createTimeEntrie));
          }
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
      const url = new URL(config.HOST + "/api/user/TimeEntries");
      url.search = new URLSearchParams(params).toString();

      try {
        const res = await adAuthenticatedFetch(url.toString());
        const timeEntries = await res.json();
        if (!Array.isArray(timeEntries) && timeEntries.message) {
          throw Error(timeEntries.message);
        }
        commit("UPDATE_TIME_ENTRIES", timeEntries.map(createTimeEntrie));
      } catch (e) {
        console.error(e);
        commit("ADD_TO_ERROR_LIST", e);
      }
    },
  },
};

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
  const isMoreThanOneComma =
    // @ts-ignore
    str.match(/[.,]/g) && str.match(/[.,]/g).length > 1;
  const isOnlyDigitsAndComma = !str.match(/[^0-9.,]/g);
  return isOnlyDigitsAndComma && !isMoreThanOneComma;
}

function shouldBeStoredServerSide(paramEntrie: ServerSideTimeEntrie) {
  return !isNonEntrieSetToZero(paramEntrie);
}

function isNonEntrieSetToZero(paramEntrie: ServerSideTimeEntrie) {
  return paramEntrie.value === 0 && paramEntrie.id === 0;
}
