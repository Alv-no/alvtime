import { State } from "./index";
import { Route } from "vue-router";
import moment from "moment";

export interface InteractionState {
  oldState: string;
  newState: string;
}

export interface AppState {
  activeDate: moment.Moment;
  activeTaskId: number;
  selectFavorites: boolean;
  isOnline: boolean;
  interactionState: InteractionState;
  editing: boolean;
  drawerOpen: boolean;
  currentRoute: Route;
}

const state = {
  activeDate: moment(),
  activeTaskId: -1,
  selectFavorites: false,
  isOnline: true,
  interactionState: { oldState: "", newState: "" },
  editing: false,
  drawerOpen: false,
  currentRoute: {} as Route,
};

const mutations = {
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

  UPDATE_INTERACTION_STATE(
    state: State,
    { oldState, newState }: InteractionState
  ) {
    state.interactionState = { oldState, newState };
  },

  UPDATE_EDITING(state: State, editing: boolean) {
    if (state.editing !== editing) {
      state.editing = editing;
    }
  },

  TOGGLE_DRAWER(state: State) {
    state.drawerOpen = !state.drawerOpen;
  },

  SET_CURRENT_ROUTE(state: State, route: Route) {
    state.currentRoute = route;
  },
};

export default {
  state,
  mutations,
};
