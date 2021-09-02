import authService from "@/services/auth";
import { State } from "./index";
import { AccountInfo } from "@azure/msal-browser";

export interface AuthState {
  account: AccountInfo | null;
  userNotFound: boolean;
}

const state = {
  account: authService.getAccount(),
  userNotFound: false,
};

const getters = {
  isValidUser: (state: State) => {
    return !!state.tasks.length;
  },
};

const mutations = {
  SET_USER_NOT_FOUND(state: State) {
    state.userNotFound = true;
  },

  SET_ACCOUNT(state: State, account: AccountInfo) {
    state.account = account;
  },
};

export default { state, getters, mutations };
