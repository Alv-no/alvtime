import { State } from "./index";
import { ActionContext } from "vuex";
import config from "@/config";
import httpClient from "../services/httpClient";

export interface InvoiceRateState {
  invoiceState: InvoiceRateModel;
}

interface InvoiceRateModel {
  invoiceRate: number;
}

const initState: InvoiceRateModel = {
  invoiceRate: 0,
};

const state: InvoiceRateState = {
  invoiceState: initState,
};

const getters = {
  getInvoiceRate: (state: State) => {
    return state.invoiceState.invoiceRate;
  },
};

const mutations = {
  SET_INVOICE_RATE(state: State, invoiceRate: number) {
    state.invoiceState.invoiceRate = Math.round(invoiceRate * 1000) / 10;
  },
};

const actions = {
  FETCH_INVOICE_RATE: ({ commit }: ActionContext<State, State>) => {
    return httpClient
      .get(`${config.API_HOST}/api/user/InvoiceRate`)
      .then(response => {
        commit("SET_INVOICE_RATE", response.data);
      });
  },
};

export default {
  state,
  getters,
  mutations,
  actions,
};
