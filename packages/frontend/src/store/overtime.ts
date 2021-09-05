import { State } from "./index";
import config from "@/config";
import { ActionContext } from "vuex";
import { groupBy } from "lodash";
import httpClient from "../services/httpClient";

export interface OvertimeData {
  key: string;
  name: string;
  colorValue: string;
  value: number;
  priority: number;
}

export interface OvertimeState {
  overtimeState: OvertimeStateModel;
}

export interface OvertimeStateModel {
  availableHours: CompansatedTransactions[];
  payoutTransactions: PayoutTransaction[];
  flexTransactions: FlexHoursTransaction[];
  totalHours: number;
  compansatedHours: number;
  totalFlexedHours: number;
  totalPayoutHours: number;
}

export interface MappedOvertimeTransaction {
  transaction: OvertimeEntry;
  type: string;
}

interface CompansatedTransactions {
  date: Date;
  hours: number;
  taskId: number;
  compensationRate: number;
}

interface AvailableHoursResponse {
  availableHoursBeforeCompensation: number;
  availableHoursAfterCompensation: number;
  entries: CompansatedTransactions[];
}

interface FlexHoursTransaction {
  date: Date;
  hours: number;
}

interface OvertimeEntry {
  date: Date;
  hours: number;
  id?: number;
  rate?: number;
  active?: boolean;
}

export interface HolidayTransaction {
  user: number;
  userEmail: string;
  id: number;
  date: Date;
  value: number;
  taskId: number;
}

export interface MappedOvertimeTransaction {
  transaction: OvertimeEntry;
  type: string;
}

interface FlexedHoursReponse {
  totalHours: number;
  entries: FlexHoursTransaction[];
}

interface PayoutReponse {
  totalHours: number;
  entries: PayoutTransaction[];
}

interface PayoutTransaction {
  id: number;
  date: Date;
  hoursBeforeCompRate: number;
  hoursAfterCompRate: number;
  active: boolean;
}

const initState: OvertimeStateModel = {
  availableHours: [],
  payoutTransactions: [],
  flexTransactions: [],
  totalHours: 0,
  compansatedHours: 0,
  totalFlexedHours: 0,
  totalPayoutHours: 0,
};

const state: OvertimeState = {
  overtimeState: initState,
};

const getters = {
  getCategorizedFlexHours: (state: State) => {
    const categorizedHours: OvertimeData[] = [];

    const groupedTransactions = groupBy(
      state.overtimeState.availableHours,
      transaction => transaction.compensationRate
    );

    for (let key in groupedTransactions) {
      let category: OvertimeData = {
        key: "",
        priority: 1,
        colorValue: "",
        value: 0,
        name: "",
      };

      switch (key) {
        case "0.5":
          category.key = "internal";
          category.colorValue = "#E8B925";
          category.name = "Frivillig";
          category.priority = 3;
          break;
        case "1":
          category.key = "internal";
          category.colorValue = "#1D92CE";
          category.name = "Alvtimer";
          category.priority = 2;
          break;
        case "1.5":
          category.key = "billable";
          category.colorValue = "#00B050";
          category.name = "Fakturerbar";
          category.priority = 1;
          break;
        default:
          console.error("Unknown hours-rate", key);
      }
      categorizedHours.push(category);
      groupedTransactions[key].forEach(item => (category.value += item.hours));
    }

    return categorizedHours;
  },
  getTransactionList: (state: State) => {
    const transactions: MappedOvertimeTransaction[] = [];

    transactions.push(
      ...state.overtimeState.availableHours
        .filter(transaction => transaction.taskId !== 0)
        .map(transaction => {
          return {
            type: "available",
            transaction: {
              date: transaction.date,
              hours: transaction.hours,
              rate: transaction.compensationRate,
            },
          };
        })
    );
    transactions.push(
      ...state.overtimeState.payoutTransactions.map(transaction => {
        return {
          type: "payout",
          transaction: {
            date: transaction.date,
            hours: transaction.hoursBeforeCompRate,
            active: transaction.active,
            id: transaction.id,
          },
        };
      })
    );
    transactions.push(
      ...state.overtimeState.flexTransactions.map(transaction => {
        return { type: "flex", transaction: transaction };
      })
    );
    return transactions.filter(
      transaction => transaction.transaction.hours != 0
    );
  },
  getAvailableHours: (state: State) => {
    return state.overtimeState.totalHours;
  },
  getAvailableCompensated: (state: State) => {
    return state.overtimeState.compansatedHours;
  },
};

const actions = {
  FETCH_AVAILABLE_HOURS: async ({ commit }: ActionContext<State, State>) => {
    await httpClient
      .get(`${config.API_HOST}/api/user/AvailableHours`)
      .then(response => {
        commit("SET_AVAILABLEHOURS", response.data);
      });
  },
  FETCH_FLEX_TRANSACTIONS: async ({ commit }: ActionContext<State, State>) => {
    await httpClient
      .get(`${config.API_HOST}/api/user/FlexedHours`)
      .then(response => {
        commit("SET_FLEXHOURS", response.data);
      });
  },
  FETCH_PAYED_HOURS: async ({ commit }: ActionContext<State, State>) => {
    await httpClient
      .get(`${config.API_HOST}/api/user/Payouts`)
      .then(response => {
        commit("SET_PAYED_HOURS", response.data);
      });
  },
  FETCH_TRANSACTIONS: async ({ dispatch }: ActionContext<State, State>) => {
    await Promise.all([
      dispatch("FETCH_AVAILABLE_HOURS"),
      dispatch("FETCH_PAYED_HOURS"),
      dispatch("FETCH_FLEX_TRANSACTIONS"),
    ]);
  },

  POST_ORDER_PAYOUT: async (
    _: ActionContext<State, State>,
    parameters: { hours: number; date: string }
  ) => {
    await httpClient
      .post(`${config.API_HOST}/api/user/Payouts`, {
        date: parameters.date,
        hours: parameters.hours,
      })
      .then(response => {
        if (response.status !== 200) throw Error(`${response.statusText}`);
      });
  },
  CANCEL_PAYOUT_ORDER: async (
    _: ActionContext<State, State>,
    parameters: { payoutId: number }
  ) => {
    await httpClient.delete(
      `${config.API_HOST}/api/user/Payouts?payoutId=${parameters.payoutId}`
    );
  },
};

const mutations = {
  SET_FLEXHOURS(state: State, transactions: FlexedHoursReponse) {
    state.overtimeState.flexTransactions = transactions.entries;
  },
  SET_PAYED_HOURS(state: State, transactions: PayoutReponse) {
    state.overtimeState.payoutTransactions = transactions.entries;
  },
  SET_AVAILABLEHOURS(state: State, transactions: AvailableHoursResponse) {
    state.overtimeState.availableHours = transactions.entries;
    state.overtimeState.totalHours =
      transactions.availableHoursBeforeCompensation;
    state.overtimeState.compansatedHours =
      transactions.availableHoursAfterCompensation;
  },
};

export default {
  state,
  getters,
  mutations,
  actions,
};
