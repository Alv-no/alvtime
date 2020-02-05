import Vue from "vue";
import Vuex from "vuex";
import timeEntrieHandlers from "./timeEntries";
import taskHandlers from "./tasks";

Vue.use(Vuex);

export interface TimeEntrie {
  id: number;
  date: string;
  value: string;
  taskId: number;
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

export interface State {
  tasks: Task[];
  timeEntries: TimeEntrie[];
  activeSlideIndex: number;
  pushQueue: TimeEntrie[];
}

export default new Vuex.Store({
  strict: process.env.NODE_ENV !== "production",
  state: {
    ...timeEntrieHandlers.state,
    ...taskHandlers.state,

    activeSlideIndex: 3,
  },
  getters: {
    ...timeEntrieHandlers.getters,
    ...taskHandlers.getters,
  },
  mutations: {
    ...timeEntrieHandlers.mutations,
    ...taskHandlers.mutations,

    UPDATE_ACTVIE_SLIDE(state: State, activeSlideIndex: number) {
      state.activeSlideIndex = activeSlideIndex;
    },
  },
  actions: {
    ...timeEntrieHandlers.actions,
    ...taskHandlers.actions,
  },
  modules: {},
});
