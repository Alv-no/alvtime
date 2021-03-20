import { State } from "./index";
import config from "@/config";
import { adAuthenticatedFetch } from "@/services/auth";
import { ActionContext } from "vuex";

export interface VacationState {
  vacationState: VacationModel;
}

interface VacationModel {
  vacationEntries: EntriesModel[];
  totalVacationHoursUsed: number;
  totalVacationDaysUsed: number;
}

interface VacationTimeModel {
  totalHoursUsed: number;
  totalDaysUsed: number;
  entries: EntriesModel[];
}

interface EntriesModel {
  user: number;
  userEmail: string;
  id: number;
  date: Date;
  value: number;
  taskId: number;
}

const initState: VacationModel = {
  vacationEntries: [],
  totalVacationDaysUsed: 0,
  totalVacationHoursUsed: 0,
};

const state: VacationState = {
  vacationState: initState,
};

const getters = {
  getUsedVacationEntries: (state: State) => {
    return state.vacationState.vacationEntries;
  },
  getUsedVacationHours: (state: State) => {
    return state.vacationState.totalVacationHoursUsed;
  },
  getUsedVacationDays: (state: State) => {
    return state.vacationState.totalVacationDaysUsed;
  },
};

const actions = {
  FETCH_USED_VACATION: async (
    { commit }: ActionContext<State, State>,
    parameters: { year: number }
  ) => {
    try {
      let url = new URL(
        config.API_HOST + `/api/user/UsedVacation?year=${parameters.year}`
      ).toString();
      const response = await adAuthenticatedFetch(url);
      const data = await response.json();
      if (response.status === 404) {
        commit("SET_USER_NOT_FOUND");
        throw response.statusText;
      }
      commit("SET_USEDVACATION", data);
    } catch (e) {
      console.error(e);
    }
  },
};

const mutations = {
  SET_USEDVACATION(state: State, transactions: VacationTimeModel) {
    state.vacationState.vacationEntries = transactions.entries;
    state.vacationState.totalVacationHoursUsed = transactions.totalHoursUsed;
    state.vacationState.totalVacationDaysUsed = transactions.totalDaysUsed;
  },
};

export default {
  state,
  getters,
  mutations,
  actions,
};
