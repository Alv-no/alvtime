import { setRedirectCallback } from "@/services/auth";
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
import vacation, { VacationState } from "./vacation";

Vue.use(Vuex);

export interface State
  extends TaskState,
    TimeEntrieState,
    AuthState,
    ErrorState,
    SwiperState,
    OvertimeState,
    VacationState,
    AppState {}

export const state = {
  ...timeEntrie.state,
  ...task.state,
  ...auth.state,
  ...error.state,
  ...swiper.state,
  ...overtime.state,
  ...vacation.state,
  ...app.state,
};

export const mutations = {
  ...timeEntrie.mutations,
  ...task.mutations,
  ...auth.mutations,
  ...error.mutations,
  ...swiper.mutations,
  ...overtime.mutations,
  ...vacation.mutations,
  ...app.mutations,
};

const getters = {
  ...task.getters,
  ...swiper.getters,
  ...auth.getters,
  ...overtime.getters,
  ...vacation.getters,
};

const actions = {
  ...timeEntrie.actions,
  ...task.actions,
  ...swiper.actions,
  ...overtime.actions,
  ...vacation.actions,
};

const storeOptions = {
  state,
  getters,
  mutations,
  actions,
};

const store = new Vuex.Store(storeOptions);

setRedirectCallback(
  (errorMessage: Error) => {
    console.error(errorMessage);
    store.commit("ADD_TO_ERROR_LIST", errorMessage);
  },
  (account: Account) => {
    store.commit("SET_ACCOUNT", account);
    router.push("hours");
  }
);

lifecycle.addEventListener("statechange", function(event: any) {
  store.commit("UPDATE_INTERACTION_STATE", event);
});

export default store;
