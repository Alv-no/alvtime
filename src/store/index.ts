import Vue from "vue";
import Vuex from "vuex";
import timeEntrie, { TimeEntrieState } from "./timeEntries";
import task, { TaskState } from "./tasks";
import auth, { AuthState } from "./auth";
import error, { ErrorState } from "./error";
import swiper, { SwiperState } from "./swiper";
import app, { AppState } from "./app";
import lifecycle from "@/services/lifecycle.es5.js";
import { setRedirectCallback } from "@/services/auth";

Vue.use(Vuex);

export interface State
  extends TaskState,
    TimeEntrieState,
    AuthState,
    ErrorState,
    SwiperState,
    AppState {}

export const state = {
  ...timeEntrie.state,
  ...task.state,
  ...auth.state,
  ...error.state,
  ...swiper.state,
  ...app.state,
};

export const mutations = {
  ...timeEntrie.mutations,
  ...task.mutations,
  ...auth.mutations,
  ...error.mutations,
  ...swiper.mutations,
  ...app.mutations,
};

const getters = {
  ...timeEntrie.getters,
  ...task.getters,
  ...swiper.getters,
  ...auth.getters,
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
};

const store = new Vuex.Store(storeOptions);

setRedirectCallback((errorMessage: Error) =>
  store.commit("ADD_TO_ERROR_LIST", errorMessage)
);

lifecycle.addEventListener("statechange", function(event: any) {
  store.commit("UPDATE_INTERACTION_STATE", event);
});

export default store;
