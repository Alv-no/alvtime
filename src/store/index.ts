import Vue from "vue";
import Vuex from "vuex";
import moment from "moment";
import timeEntrieHandlers from "./timeEntries";
import taskHandlers from "./tasks";
import auth from "./auth";
import error from "./error";
// @ts-ignore
import lifecycle from "@/services/lifecycle.es5.js";

moment.locale("nb");

Vue.use(Vuex);

export interface FrontendTimentrie {
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
  timeEntries: FrontendTimentrie[];
  activeDate: moment.Moment;
  activeTaskId: number;
  pushQueue: FrontendTimentrie[];
  selectFavorites: boolean;
  account: Account | null;
  isOnline: boolean;
  errorTexts: string[];
  appState: { oldState: string; newState: string };
}

const store = new Vuex.Store({
  strict: process.env.NODE_ENV !== "production",
  state: {
    ...timeEntrieHandlers.state,
    ...taskHandlers.state,
    ...auth.state,
    ...error.state,

    appState: { oldState: "", newState: "" },
    isOnline: true,
    activeDate: moment(),
    activeTaskId: -1,
    selectFavorites: false,
  },
  getters: {
    ...timeEntrieHandlers.getters,
    ...taskHandlers.getters,
  },
  mutations: {
    ...timeEntrieHandlers.mutations,
    ...taskHandlers.mutations,
    ...error.mutations,

    UPDATE_ACTVIE_DATE(state: State, date: moment.Moment) {
      state.activeDate = date;
    },

    UPDATE_ACTVIE_TASK(state: State, taskId: number) {
      state.activeTaskId = taskId;
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

    UPDATE_APP_STATE(state: State, { oldState, newState }) {
      state.appState = { oldState, newState };
    },
  },
  actions: {
    ...timeEntrieHandlers.actions,
    ...taskHandlers.actions,
  },
  modules: {},
});

lifecycle.addEventListener("statechange", function(event: any) {
  store.commit("UPDATE_APP_STATE", event);
});

export default store;
