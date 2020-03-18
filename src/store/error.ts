import { State } from "./index";

export default {
  state: {
    errorTexts: [],
  },

  mutations: {
    ADD_TO_ERROR_LIST(state: State, error: Error) {
      state.errorTexts = [...state.errorTexts, error.message];
    },

    CLEAR_ERROR_LIST(state: State) {
      state.errorTexts = [];
    },
  },
};
