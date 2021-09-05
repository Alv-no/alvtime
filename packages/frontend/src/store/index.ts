import authService from "@/services/auth";
import lifecycle from "@/services/lifecycle.es5.js";
import Vue from "vue";
import Vuex from "vuex";
import app, { AppState } from "./app";
import auth, { AuthState } from "./auth";
import error, { ErrorState } from "./error";
import swiper, { SwiperState } from "./swiper";
import task, { TaskState } from "./tasks";
import timeEntrie, { TimeEntrieState } from "./timeEntries";
import router from "@/router";
import overtime, { OvertimeState } from "./overtime";
import absense, { AbsenseState } from "./absense";
import { EventMessage, EventType } from "@azure/msal-browser";
import { registerErrorCallback } from "@/services/httpClient";

Vue.use(Vuex);

export interface State
  extends TaskState,
    TimeEntrieState,
    AuthState,
    ErrorState,
    SwiperState,
    OvertimeState,
    AbsenseState,
    AppState {}

export const state = {
  ...timeEntrie.state,
  ...task.state,
  ...auth.state,
  ...error.state,
  ...swiper.state,
  ...overtime.state,
  ...app.state,
  ...absense.state,
};

export const mutations = {
  ...timeEntrie.mutations,
  ...task.mutations,
  ...auth.mutations,
  ...error.mutations,
  ...swiper.mutations,
  ...overtime.mutations,
  ...app.mutations,
  ...absense.mutations,
};

const getters = {
  ...task.getters,
  ...swiper.getters,
  ...auth.getters,
  ...overtime.getters,
  ...absense.getters,
  ...error.getters,
};

const actions = {
  ...timeEntrie.actions,
  ...task.actions,
  ...swiper.actions,
  ...overtime.actions,
  ...absense.actions,
  ...auth.actions,
};

const storeOptions = {
  state,
  getters,
  mutations,
  actions,
};

registerErrorCallback(e => {
  if (e.status === 404 && e.message === "User not found") {
    return;
  }
  store.commit("ADD_TO_ERROR_LIST", e);
});

const store = new Vuex.Store(storeOptions);
authService.getAccountAsync().then(accountInfo => {
  store.commit("SET_ACCOUNT", accountInfo);
});

authService.addCallback((message: EventMessage) => {
  if (message.eventType.endsWith("Failure")) {
    store.commit("ADD_TO_ERROR_LIST", message.error);
  } else if (message.eventType === EventType.LOGIN_SUCCESS) {
    router.push("hours");
  }
});

lifecycle.addEventListener("statechange", function(event: any) {
  store.commit("UPDATE_INTERACTION_STATE", event);
});

export default store;
