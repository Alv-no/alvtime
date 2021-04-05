import {State} from './index';
import { adAuthenticatedFetch } from "@/services/auth";
import config from "@/config";
import { ActionContext } from "vuex";

interface AbsenseOverview {
  vacationDays: number;
  usedVacationDays: number;
  absenseDaysInAYear: number;
  usedAbsenseDays: number;
  alvDaysInAYear: number;
  usedAlvDays: number;
}


export interface AbsenseState {
  absenseState: AbsenseStateModel;
}

export interface AbsenseStateModel {
  absenseOverview: AbsenseOverview;
}

const initState: AbsenseStateModel = {
  absenseOverview: {
    vacationDays: 0,
    usedVacationDays: 0,
    absenseDaysInAYear: 0,
    usedAbsenseDays: 0,
    alvDaysInAYear: 0,
    usedAlvDays: 0
  }
}

const state: AbsenseState = {
  absenseState: initState
}

const getters = {
  getAbsenseOverview: (state: State) => {
    return [
      {
        key: 'vacation',
        name: 'Feriedager',
        colorValue: '#00B050',
        value: state.absenseState.absenseOverview.vacationDays,
        priority: 2
      },
      {
        key: 'absensedays',
        name: 'Egenmeldingsdager',
        colorValue: '#E8B925',
        value: state.absenseState.absenseOverview.absenseDaysInAYear,
        priority: 1
      },
      {
        key: 'alvdays',
        name: 'Alvdager',
        colorValue: '#1D92CE',
        value: state.absenseState.absenseOverview.alvDaysInAYear,
        priority: 3
      }
    ]
    //return state.absenseState.absenseOverview;
  },
  getAbsenseOverviewSubtractions: (state: State) => {
    return [
      {
        key: 'vacation',
        value: state.absenseState.absenseOverview.usedVacationDays
      },
      {
        key: 'absensedays',
        value: state.absenseState.absenseOverview.usedAbsenseDays
      },
      {
        key: 'alvdays',
        value: state.absenseState.absenseOverview.usedAlvDays
      }
    ]
  }
};

const mutations = {
  SET_ABSENSEDAYS(state: State, absenseOverview: AbsenseOverview) {
    console.log(absenseOverview);
    state.absenseState.absenseOverview = absenseOverview;
  }
};

const actions = {
  FETCH_ABSENSEDATA: async ({ commit }: ActionContext<State, State>) => {
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
      commit("SET_ABSENSEDAYS", available);
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
  actions
};
