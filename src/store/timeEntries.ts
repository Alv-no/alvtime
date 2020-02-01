import moment from "moment";
import { State, TimeEntrie } from "./index";
import { ActionContext } from "vuex";
import { debounce } from "lodash";

export interface ServerSideTimeEntrie {
  id: number;
  date: string;
  value: number;
  taskId: number;
}

export default {
  state: {
    timeEntries: [
      {
        id: 1,
        date: moment()
          .add(-1, "day")
          .format("YYYY-MM-DD"),
        value: 7.5,
        taskId: 4,
      },
      {
        id: 2,
        date: moment().format("YYYY-MM-DD"),
        value: 7.5,
        taskId: 4,
      },
      {
        id: 3,
        date: moment()
          .add(1, "day")
          .format("YYYY-MM-DD"),
        value: 7.5,
        taskId: 4,
      },
      {
        id: 4,
        date: "2020-01-18",
        value: 7.5,
        taskId: 4,
      },
    ].map(createTimeEntrie),

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
          const timeEntries = await mockPost(timeEntriesToPush);
          commit("UPDATE_TIME_ENTRIES", timeEntries);
        } catch (e) {
          console.error(e);
        }
      },
      1000
    ),

    FETCH_TIME_ENTRIES: async () => {
      const url = new URL("http://localhost/api/user/TimeEntries");
      const params = {
        fromDateInclusive: "2019-01-09",
        toDateInclusive: "2020-01-09",
      };
      url.search = new URLSearchParams(params).toString();
      const res = await fetch(url.toString());
      const timeEntries = await res.json();
      console.log("tasks: ", timeEntries);
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

const mockPost = (timeEntries: ServerSideTimeEntrie[]) =>
  new Promise(resolve => setTimeout(() => resolve(timeEntries), 200));

function isMatchingEntrie(entrieA: TimeEntrie, entrieB: TimeEntrie) {
  return entrieA.date === entrieB.date && entrieA.taskId === entrieB.taskId;
}

function createTimeEntrie(data: any): TimeEntrie {
  return { ...data, value: data.value.toString() };
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
