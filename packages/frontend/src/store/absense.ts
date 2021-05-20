import { State } from "./index";
import { adAuthenticatedFetch } from "@/services/auth";
import config from "@/config";
import { ActionContext } from "vuex";

interface VacationOverview {
  availableVacationDays: number;
  plannedVacationDays: number;
  usedVacationDays: number;
}

interface AbsenseOverview {
  absenseDaysInAYear: number;
  usedAbsenseDays: number;
}

export interface AbsenseState {
  absenseState: AbsenseStateModel;
}

export interface AbsenseStateModel {
  vacationOverview: VacationOverview;
  absenseOverview: AbsenseOverview;
}

const initState: AbsenseStateModel = {
  vacationOverview: {
    usedVacationDays: 0,
    plannedVacationDays: 0,
    availableVacationDays: 0,
  },
  absenseOverview: {
    absenseDaysInAYear: 0,
    usedAbsenseDays: 0,
  },
};

const state: AbsenseState = {
  absenseState: initState,
};

const getters = {
  getAbsenseOverview: (state: State) => {
    return [
      {
        name: "Planlagt",
        colorValue: "#00B050",
        value: state.absenseState.vacationOverview.plannedVacationDays,
        priority: 2,
      },
      {
        name: "Brukt",
        colorValue: "#E8B925",
        value: state.absenseState.vacationOverview.usedVacationDays,
        priority: 1,
      },
      {
        name: "Tilgjengelig",
        colorValue: "#1D92CE",
        value: state.absenseState.vacationOverview.availableVacationDays,
        priority: 3,
      },
    ];
  },
};

const mutations = {
  SET_ABSENSEDAYSOVERVIEW(state: State, absenseOverview: AbsenseOverview) {
    state.absenseState.absenseOverview = absenseOverview;
  },
  SET_VACATIONOVERVIEW(state: State, vacationOverview: VacationOverview) {
    state.absenseState.vacationOverview = vacationOverview;
  },
};

const actions = {
  FETCH_ABSENSEDATAOVERVIEW: async ({
    commit,
  }: ActionContext<State, State>) => {
    try {
      let url = new URL(
        config.API_HOST + "/user/RemainingAbsenseDays"
      ).toString();
      let res = await adAuthenticatedFetch(url);
      if (res.status === 404) {
        commit("SET_USER_NOT_FOUND");
        throw res.statusText;
      }
      const available = await res.json();
      commit("SET_ABSENSEDAYSOVERVIEW", available);
    } catch (e) {
      if (e !== "Not Found") {
        console.error(e);
        commit("ADD_TO_ERROR_LIST", e);
      }
    }
  },
  FETCH_VACATIONOVERVIEW: async ({ commit }: ActionContext<State, State>) => {
    try {
      let url = new URL(
        config.API_HOST + "/api/user/VacationOverview"
      ).toString();
      let res = await adAuthenticatedFetch(url);
      if (res.status === 404) {
        commit("SET_USER_NOT_FOUND");
        throw res.statusText;
      }
      const available = await res.json();
      commit("SET_VACATIONOVERVIEW", available);
    } catch (e) {
      if (e !== "Not Found") {
        console.error(e);
        commit("ADD_TO_ERROR_LIST", e);
      }
    }
  },
};

export default {
  state,
  getters,
  mutations,
  actions,
};
