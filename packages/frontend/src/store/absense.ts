import { State } from "./index";
import config from "@/config";
import { ActionContext } from "vuex";
import httpClient from "../services/httpClient";

const AVAILABLE_HOURS_INDEX = 2;
const AVAILABLE_HOURS_PREVIOUS_YEAR = 3;

interface VacationOverview {
  availableVacationDays: number;
  plannedVacationDaysThisYear: number;
  usedVacationDaysThisYear: number;
  availableVacationDaysTransferredFromLastYear: number;
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
    usedVacationDaysThisYear: 0,
    plannedVacationDaysThisYear: 0,
    availableVacationDays: 0,
    availableVacationDaysTransferredFromLastYear: 0,
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
    const absenseOverview = generateAbsenseOverview(state);
    absenseOverview[AVAILABLE_HOURS_PREVIOUS_YEAR].value = 0;
    absenseOverview[AVAILABLE_HOURS_PREVIOUS_YEAR].value = 0;
    absenseOverview[AVAILABLE_HOURS_PREVIOUS_YEAR].name = "";
    return absenseOverview;
  },
  getAbsenseOverviewSplit: (state: State) => {
    const absenseOverview = generateAbsenseOverview(state);
    const transferredHours =
      absenseOverview[AVAILABLE_HOURS_PREVIOUS_YEAR].value;
    const yearlyHours = absenseOverview[AVAILABLE_HOURS_INDEX].value;
    absenseOverview[AVAILABLE_HOURS_INDEX].value;
    absenseOverview[AVAILABLE_HOURS_INDEX].value =
      yearlyHours - transferredHours;

    if (transferredHours !== 0) {
      absenseOverview[AVAILABLE_HOURS_INDEX].name = "Opptjent";
    }
    return absenseOverview;
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
  FETCH_ABSENSEDATAOVERVIEW: ({ commit }: ActionContext<State, State>) => {
    return httpClient
      .get(`${config.API_HOST}/api/user/AbsenseOverview`)
      .then(response => {
        commit("SET_ABSENSEDAYSOVERVIEW", response.data);
      });
  },
  FETCH_VACATIONOVERVIEW: ({ commit }: ActionContext<State, State>) => {
    return httpClient
      .get(`${config.API_HOST}/api/user/VacationOverview`)
      .then(response => {
        commit("SET_VACATIONOVERVIEW", response.data);
      });
  },
};

const generateAbsenseOverview = (state: State) => {
  return [
    {
      name: "Planlagt i år",
      colorValue: "#00B050",
      value: state.absenseState.vacationOverview.plannedVacationDaysThisYear,
      priority: 3,
    },
    {
      name: "Brukt i år",
      colorValue: "#E8B925",
      value: state.absenseState.vacationOverview.usedVacationDaysThisYear,
      priority: 4,
    },
    {
      name: "Tilgjengelig",
      colorValue: "#1D92CE",
      value: state.absenseState.vacationOverview.availableVacationDays,
      priority: 1,
    },
    {
      name: "Overført fra i fjor",
      colorValue: "#ea899a",
      value:
        state.absenseState.vacationOverview
          .availableVacationDaysTransferredFromLastYear,
      priority: 2,
    },
  ];
};

export default {
  state,
  getters,
  mutations,
  actions,
};
