import moment from "moment";
import timeEntrieHandlers from "./timeEntries";
import taskHandlers from "./tasks";
import auth from "./auth";
import error from "./error";

moment.locale("nb");

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

export interface TimeEntrieObj {
  value: string;
  id: number;
}

export interface TimeEntrieMap {
  [key: string]: TimeEntrieObj;
}

interface AppState {
  oldState: string;
  newState: string;
}

export interface State {
  tasks: Task[];
  timeEntries: FrontendTimentrie[];
  timeEntriesMap: TimeEntrieMap;
  activeDate: moment.Moment;
  activeTaskId: number;
  pushQueue: FrontendTimentrie[];
  selectFavorites: boolean;
  account: Account | null;
  isOnline: boolean;
  errorTexts: string[];
  appState: AppState;
  editing: boolean;
}

export const mutations = {
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

  UPDATE_APP_STATE(state: State, { oldState, newState }: AppState) {
    state.appState = { oldState, newState };
  },

  UPDATE_EDITING(state: State, editing: boolean) {
    if (state.editing !== editing) {
      state.editing = editing;
    }
  },
};

export const state = {
  ...timeEntrieHandlers.state,
  ...taskHandlers.state,
  ...auth.state,
  ...error.state,

  appState: { oldState: "", newState: "" },
  isOnline: true,
  activeDate: moment(),
  activeTaskId: -1,
  selectFavorites: false,
  editing: false,
};

export default {
  strict: process.env.NODE_ENV !== "production",
  state,
  getters: {
    ...timeEntrieHandlers.getters,
    ...taskHandlers.getters,
  },
  mutations,
  actions: {
    ...timeEntrieHandlers.actions,
    ...taskHandlers.actions,
  },
  modules: {},
};
