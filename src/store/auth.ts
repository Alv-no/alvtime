import { State } from "./index";
import { getAccount } from "../services/auth";

export interface AuthState {
  account: Account | null;
  userNotFound: boolean;
}

interface Account {
  name: string;
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
};

export default { state, getters, mutations };
