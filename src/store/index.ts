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
  projectId: number;
  projectName: string;
  customerId: number;
  customerName: string;
  hourRate: number;
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
