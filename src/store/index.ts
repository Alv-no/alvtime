import Vue from "vue";
import Vuex from "vuex";
import timeEntrieHandlers from "./timeEntries";
import taskHandlers from "./tasks";
import auth from "./auth";

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

interface Account {
  name: string;
}

export interface State {
  tasks: Task[];
  timeEntries: TimeEntrie[];
  activeSlideIndex: number;
  pushQueue: TimeEntrie[];
  selectFavorites: boolean;
  account: Account | null;
  isOnline: boolean;
}

const store = new Vuex.Store({
  strict: process.env.NODE_ENV !== "production",
  state: {
    ...timeEntrieHandlers.state,
    ...taskHandlers.state,
    ...auth.state,

    isOnline: true,
    activeSlideIndex: 3,
    selectFavorites: false,
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

    TOGGLE_SELECTFAVORITES(state: State) {
      state.selectFavorites = !state.selectFavorites;
    },

    UPDATE_ONLINE_STATUS(state: State) {
      if (typeof window.navigator.onLine === "undefined") {
        // If the browser doesn't support connection status reports
        // assume that we are online because most apps' only react
        // when they now that the connection has been interrupted
        state.isOnline = true;
      } else {
        state.isOnline = window.navigator.onLine;
      }
    },
  },
  actions: {
    ...timeEntrieHandlers.actions,
    ...taskHandlers.actions,
  },
  modules: {},
});

export default store;
