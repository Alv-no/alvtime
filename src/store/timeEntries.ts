import moment from "moment";
import { State, TimeEntrie } from "./index";
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
    getTimeEntrie: (state: State) => (entrieA: TimeEntrie) => {
      return state.timeEntries.find((entrieB: TimeEntrie) =>
        isMatchingEntrie(entrieA, entrieB)
      );
    },
  },

  mutations: {
    SET_TIME_ENTRIES(state: State, paramEntries: TimeEntrie[]) {
      state.timeEntries = paramEntries;
    },

    UPDATE_TIME_ENTRIES(state: State, paramEntries: TimeEntrie[]) {
      for (const paramEntrie of paramEntries) {
        state.timeEntries = updateArrayWith(state.timeEntries, paramEntrie);
      }
    },

    ADD_TO_PUSH_QUEUE(state: State, paramEntrie: TimeEntrie) {
      state.pushQueue = updateArrayWith(state.pushQueue, paramEntrie);
    },

    FLUSH_PUSH_QUEUE(state: State) {
      state.pushQueue = [];
    },
  },

  actions: {
    UPDATE_TIME_ENTRIE(
      { commit, dispatch }: ActionContext<State, State>,
      paramEntrie: TimeEntrie
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
          console.log("timeEntries: ", timeEntries);
          console.log("response: ", response);
          if (response.status !== 200) {
            throw Error(`${response.statusText}
${timeEntries.title}`);
          }
          console.log("timeEntries: ", timeEntries);
          commit("UPDATE_TIME_ENTRIES", timeEntries.map(createTimeEntrie));
        } catch (e) {
          console.error(e);
          commit("ADD_TO_ERROR_LIST", e);
        }
      },
      1000
    ),

    FETCH_TIME_ENTRIES: async ({ commit }: ActionContext<State, State>) => {
      const url = new URL(config.HOST + "/api/user/TimeEntries");
      const params = {
        fromDateInclusive: moment()
          .add(-2, "week")
          .startOf("week")
          .format(config.DATE_FORMAT),
        toDateInclusive: moment()
          .add(2, "week")
          .endOf("week")
          .format(config.DATE_FORMAT),
      };
      url.search = new URLSearchParams(params).toString();

      try {
        const res = await adAuthenticatedFetch(url.toString());
        const timeEntries = await res.json();
        commit("SET_TIME_ENTRIES", timeEntries.map(createTimeEntrie));
      } catch (e) {
        console.error(e);
        commit("ADD_TO_ERROR_LIST", e);
      }
    },
  },
};

function updateArrayWith(arr: TimeEntrie[], paramEntrie: TimeEntrie) {
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

function isMatchingEntrie(entrieA: TimeEntrie, entrieB: TimeEntrie) {
  return entrieA.date === entrieB.date && entrieA.taskId === entrieB.taskId;
}

function createTimeEntrie(data: any): TimeEntrie {
  return {
    ...data,
    date: data.date.split("T")[0],
    value: data.value.toString(),
  };
}

function createServerSideTimeEntrie(timeEntrie: TimeEntrie) {
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
