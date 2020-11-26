import { State } from "./index";

export interface ErrorState {
  errorTexts: string[];
}

const state = {
  errorTexts: [],
};

const mutations = {
  ADD_TO_ERROR_LIST(state: State, error: Error) {
    state.errorTexts = [...state.errorTexts, error.message];
  },

  CLEAR_ERROR_LIST(state: State) {
    state.errorTexts = [];
  },
};

export default {
  state,
  mutations,
};
