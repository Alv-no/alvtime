import authService from "@/services/auth";
import { State } from "./index";
import { AccountInfo } from "@azure/msal-browser";
import { ActionContext } from "vuex";
import httpClient from "../services/httpClient";
import config from "@/config";

export interface AuthState {
  account: AccountInfo | null;
  userNotFound: boolean;
  userDetail?: UserDetails;
}

export interface UserDetails {
  Id: number;
  Name: string;
  Email: string;
  StartDate: Date;
  EndDate?: Date;
}

const state = {
  account: authService.getAccount(),
  userNotFound: false,
};

const getters = {
  isValidUser: (state: State) => {
    return state.userDetail === undefined;
  },
  getUser: (state: State) => {
    return state.userDetail;
  },
};

const mutations = {
  SET_USER_NOT_FOUND(state: State) {
    state.userNotFound = true;
  },
  SET_ACCOUNT(state: State, account: AccountInfo) {
    state.account = account;
  },
  SET_USER_DETAIL(state: State, details: UserDetails) {
    state.userDetail = details;
  },
};

const actions = {
  FETCH_USER_DETAILS: async ({ commit }: ActionContext<State, State>) => {
    await httpClient
      .get(`${config.API_HOST}/api/user/Profile`)
      .then(response => {
        commit("SET_USER_DETAIL", response.data);
      })
      .catch(() => {
        commit("SET_USER_NOT_FOUND");
      });
  },
};

export default { state, getters, mutations, actions };
