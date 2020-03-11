import { State } from "./index";
import { ActionContext } from "vuex";
import { Account, AuthResponse } from "msal";
import { msalApp, GRAPH_REQUESTS, acquireToken } from "../services/auth";

export default {
  state: {
    account: msalApp.getAccount(),
  },

  mutations: {
    SET_ACCOUNT(state: State, account: Account) {
      state.account = account;
    },
    LOGOUT(state: State) {
      state.account = null;
      msalApp.logout();
    },
  },

  actions: {
    async LOGIN({ commit }: ActionContext<State, State>) {
      try {
        const loginResponse = await msalApp.loginPopup(GRAPH_REQUESTS.LOGIN);
        await acquireToken(GRAPH_REQUESTS.LOGIN);
        commit("SET_ACCOUNT", loginResponse.account);
      } catch (error) {
        console.error(error);
      }
    },
  },
};
