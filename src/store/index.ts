import Vue from "vue";
import Vuex from "vuex";
import timeEntrieHandlers from "./timeEntries";
import taskHandlers from "./tasks";

Vue.use(Vuex);

export interface TimeEntrie {
  id: number;
  date: string;
  value: number;
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
}

export default new Vuex.Store({
  strict: true,
  state: {
    ...timeEntrieHandlers.state,
    ...taskHandlers.state,
  },
  getters: {
    ...timeEntrieHandlers.getters,
    ...taskHandlers.getters,
  },
  mutations: {
    ...timeEntrieHandlers.mutations,
  },
  actions: {},
  modules: {},
});
