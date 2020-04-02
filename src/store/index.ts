import moment from "moment";
import timeEntrie, { TimeEntrieState } from "./timeEntries";
import task, { TaskState } from "./tasks";
import auth, { AuthState } from "./auth";
import error, { ErrorState } from "./error";
import swiper, { SwiperState } from "./swiper";

moment.locale("nb");

interface InteractionState {
  oldState: string;
  newState: string;
}

export interface State
  extends TaskState,
    TimeEntrieState,
    AuthState,
    ErrorState,
    SwiperState {
  activeDate: moment.Moment;
  activeTaskId: number;
  selectFavorites: boolean;
  isOnline: boolean;
  interactionState: InteractionState;
  editing: boolean;
}

export const state = {
  ...timeEntrie.state,
  ...task.state,
  ...auth.state,
  ...error.state,
  ...swiper.state,

  interactionState: { oldState: "", newState: "" },
  isOnline: true,
  activeDate: moment(),
  activeTaskId: -1,
  selectFavorites: false,
  editing: false,
};

export const mutations = {
  ...timeEntrie.mutations,
  ...task.mutations,
  ...error.mutations,
  ...swiper.mutations,

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

  UPDATE_APP_STATE(state: State, { oldState, newState }: InteractionState) {
    state.interactionState = { oldState, newState };
  },

  UPDATE_EDITING(state: State, editing: boolean) {
    if (state.editing !== editing) {
      state.editing = editing;
    }
  },
};

const getters = {
  ...timeEntrie.getters,
  ...task.getters,
  ...swiper.getters,
};

const actions = {
  ...timeEntrie.actions,
  ...task.actions,
  ...swiper.actions,
};

const storeOptions = {
  state,
  getters,
  mutations,
  actions,
  modules: {},
};

export default storeOptions;
