import { State } from "./index";

export interface ErrorState {
  errorTexts: string[];
  errors: { name: string; message: string; status: number }[];
}

const state = {
  errorTexts: [],
  errors: [],
};

const mutations = {
  ADD_TO_ERROR_LIST(
    state: State,
    error: { name: string; message: string; status: number }
  ) {
    state.errors = [...state.errors, error];
    state.errorTexts = [...state.errorTexts, error.message];
  },

  CLEAR_ERROR_LIST(state: State) {
    state.errorTexts = [];
  },
};

const getters = {
  getErrorMessages: (state: State) => {
    return state.errors;
  },
  getAllErrors: (state: State) => {
    return state.errors
      .map(error => `${error.status}: ${error.name} \n ${error.message}`)
      .join("-->");
  },
};

export default {
  state,
  mutations,
  getters,
};
