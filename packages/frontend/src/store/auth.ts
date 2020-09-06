import { Account } from "@azure/msal-common";
import { getAccount } from "@/services/auth";
import { State } from "./index";

export interface AuthState {
  account: Account | null;
  userNotFound: boolean;
}

const state = {
  account: getAccount(),
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

  SET_ACCOUNT(state: State, account: Account) {
    state.account = account;
  },
};

export default { state, getters, mutations };
