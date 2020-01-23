import moment from "moment";
import { State, TimeEntrie } from "./index";
import { ActionContext } from "vuex";

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
    ],
  },

  getters: {
    getTimeEntrie: (state: State) => (id: number, date: string) => {
      return state.timeEntries.find(
        (entrie: TimeEntrie) => entrie.id === id && entrie.date === date
      );
    },
  },

  mutations: {
    UPDATE_TIME_ENTRIE(state: State, timeEntrie: TimeEntrie) {
      const index = state.timeEntries.findIndex(
        entrie => entrie.id === timeEntrie.id && entrie.date === timeEntrie.date
      );

      if (index !== -1) {
        state.timeEntries = [
          ...state.timeEntries.map(entrie =>
            entrie.id !== timeEntrie.id ? entrie : timeEntrie
          ),
        ];
      } else {
        state.timeEntries = [
          ...state.timeEntries,
          {
            ...timeEntrie,
            id: Math.max(...state.timeEntries.map(entrie => entrie.id)) + 1,
          },
        ];
      }
    },
  },

  actions: {
    UPDATE_TIME_ENTRIE(
      { commit }: ActionContext<State, State>,
      timeEntrie: TimeEntrie
    ) {
      commit("UPDATE_TIME_ENTRIE", timeEntrie);
    },
  },
};
