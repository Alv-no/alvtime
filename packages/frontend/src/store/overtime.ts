import { State } from "./index";
import config from '@/config';
import {adAuthenticatedFetch} from '@/services/auth';
import {ActionContext} from 'vuex';

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

export interface CompansatedTransactions {
		date: Date;
		hours: number;
		taskId: number;
		compensationRate: number;
}

export interface AvailableHoursResponse {
		totalHours: number;
		totalHoursIncludingCompensationRate: number;
		entries: CompansatedTransactions[];
}

export interface FlexHoursTransaction {
		date: Date;
		hours: number;
		rate: number;
}

export interface Transaction {
		date: Date;
		hours: number;
		id?: number;
		rate?: number;
		active?: boolean;
}

export interface MappedTransaction {
	transaction: Transaction;
	type: string;
}

export interface FlexedHoursReponse {
		totalHours: number;
		entries: FlexHoursTransaction[];
}

export interface PayoutReponse {
		totalHours: number;
		entries: PayoutTransaction[];
}


export interface PayoutTransaction {
		date: Date;
		hours: number;
		id: number;
		active: boolean;
}

const initState: OvertimeStateModel = {
		availableHours: [],
		payoutTransactions: [],
		flexTransactions: [],
		totalHours: 0,
		compansatedHours: 0,
		totalFlexedHours: 0,
		totalPayoutHours: 0
}

const state: OvertimeState = {
	overtimeState: initState
}

const getters = {
	getTransactionList: (state: State) => {
		const transactions: MappedTransaction[] = [];

		transactions.push(...state.overtimeState.availableHours
											.map(transaction => { return {type: 'available', transaction: {
												date: transaction.date,
												hours: transaction.hours,
												rate: transaction.compensationRate
											}}}))
		transactions.push(...state.overtimeState.payoutTransactions
											.map(transaction => { return {type: 'payout', transaction: {
												date: transaction.date,
												hours: transaction.hours,
												active: transaction.active,
												id: transaction.id
											}}}))
		transactions.push(...state.overtimeState.flexTransactions.filter(transaction => transaction.hours != 0)
											.map(transaction => { return {type: 'flex', transaction: transaction}}))
		return transactions;

	},
	getAvailableHours: (state: State) => {
		return state.overtimeState.totalHours;
	},
	getAvailableCompensated: (state: State) => {
		return state.overtimeState.compansatedHours;
	},
}


const actions = {
	FETCH_TRANSACTIONS: async ({ commit }: ActionContext<State, State>) => {
		// TODO: Refactor this a big bit
    try {
      let url = new URL(config.API_HOST + "/api/user/AvailableHours").toString();
      let res = await adAuthenticatedFetch(url);
      if (res.status === 404) {
        commit("SET_USER_NOT_FOUND");
        throw res.statusText;
      }
      const available = await res.json();
      commit("SET_AVAILABLEHOURS", available);

      url = new URL(config.API_HOST + "/api/user/FlexedHours").toString();
      res = await adAuthenticatedFetch(url);
      if (res.status === 404) {
        commit("SET_USER_NOT_FOUND");
        throw res.statusText;
      }
      const flexed = await res.json();
      commit("SET_FLEXHOURS", flexed);

      url = new URL(config.API_HOST + "/api/user/Payouts").toString();
      res = await adAuthenticatedFetch(url);
      if (res.status === 404) {
        commit("SET_USER_NOT_FOUND");
        throw res.statusText;
      }

      const payed = await res.json();
      commit("SET_PAYED_HOURS", payed);

    } catch (e) {
      if (e !== "Not Found") {
        console.error(e);
        commit("ADD_TO_ERROR_LIST", e);
      }
    }
	},

	POST_ORDER_PAYOUT:  async ({ commit }: ActionContext<State, State>, parameters: {hours: number, date: string}) => {
		try {
      let url = new URL(config.API_HOST + "/api/user/Payouts").toString();
			const response = await adAuthenticatedFetch(url, {
				method: 'post',
				headers: {'Content-Type' : 'application/json'},
				body: JSON.stringify({
					date: parameters.date,
					value: parameters.hours
				})
			});
			if (response.status !== 200)
				throw Error(`${response.statusText}`);

		} catch(e) {
			console.error(e);
			commit("ADD_TO_ERROR_LIST", e);
		}
	}
}

const mutations = {
	SET_FLEXHOURS(state: State, transactions: FlexedHoursReponse) {
		state.overtimeState.flexTransactions = transactions.entries;
	},
	SET_PAYED_HOURS(state: State, transactions: PayoutReponse) {
		state.overtimeState.payoutTransactions = transactions.entries;
	},
	SET_AVAILABLEHOURS(state: State, transactions: AvailableHoursResponse) {
		state.overtimeState.availableHours = transactions.entries;
		state.overtimeState.totalHours = transactions.totalHours;
		state.overtimeState.compansatedHours = transactions.totalHoursIncludingCompensationRate;
	}
	
}

export default {
	state,
	getters,
	mutations,
	actions
}
