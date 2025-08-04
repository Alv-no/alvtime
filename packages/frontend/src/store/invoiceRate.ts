import { State } from "./index";
import { ActionContext } from "vuex";
import config from "@/config";
import httpClient from "../services/httpClient";
import {
  mapTimeStampToLabel,
  createTimeString,
  decimalToRoundedPercentage,
  formatDecimalArray,
  averageFromArray,
} from "@/utils/timestamp-text-util";

export interface InvoiceStoreState {
  invoiceState: InvoiceRateModel;
}
// add new type: list of hours separeted by type (billable, non-billable, vacation etc)
// typescript interface for that.

export enum InvoicePeriods {
  Daily = 0,
  Weekly = 1,
  Monthly = 2,
  Annually = 3,
}

export interface InvoiceStatisticsFilters {
  toDate?: string;
  fromDate?: string;
  granularity?: InvoicePeriods;
}

export enum InvoiceStatisticsPresetTypes {
  YEAR_INTERVAL,
  HALF_YEAR_INTERVAL,
  QUARTER_INTERVAL,
  WEEK_INTERVAL,
}

export interface InvoiceStatisticsPreset {
  type: InvoiceStatisticsPresetTypes;
  granularity: InvoicePeriods;
  label: string;
}

//endepunktet:
export interface InvoiceStatistics {
  billableHours: number[];
  nonBillableHours: number[];
  vacationHours: number[];
  overtimeHours: {
    invoiceableOvertimeHours: number[];
    internalOvertimeHours: number[];
  };
  invoiceRate: number[];
  nonBillableInvoiceRate: number[];
  labels: string[];
}
 
export interface SummarizedStatistics {
  title: string;
  values: { title?: string; value: number; unit?: string }[];
}

interface InvoiceRateModel {
  invoiceRate: number;
  invoiceStatistics: InvoiceStatistics;
  invoiceStatisticFilters: InvoiceStatisticsFilters;
  invoiceStatisticPreset: InvoiceStatisticsPreset | null;
}

const initStatistics: InvoiceStatistics = {
  billableHours: [],
  nonBillableHours: [],
  overtimeHours: {
	invoiceableOvertimeHours: [],
	internalOvertimeHours: [],
  },
  vacationHours: [],
  invoiceRate: [],
  nonBillableInvoiceRate: [],
  labels: [],
};

const initState: InvoiceRateModel = {
  invoiceRate: 0,
  invoiceStatistics: initStatistics,
  invoiceStatisticFilters: createDefaultStatisticInterval(),
  invoiceStatisticPreset: null,
};

const state: InvoiceStoreState = {
  invoiceState: initState,
};

const getters = {
  getInvoiceRate: (state: State) => {
    return state.invoiceState.invoiceRate;
  },
  getInvoiceStatistics: (state: State) => {
    return state.invoiceState.invoiceStatistics;
  },
  getInvoiceLabel: (state: State) => (
    valueType: 0 | 1,
    index: number
  ): number => {
    const data =
      valueType === 0
        ? state.invoiceState.invoiceStatistics.billableHours
        : state.invoiceState.invoiceStatistics.nonBillableHours;
    return data[index];
  },
  getInvoiceFilters: (state: State): InvoiceStatisticsFilters => {
    return state.invoiceState.invoiceStatisticFilters;
  },
  getSummarizedStatistics: (state: State): SummarizedStatistics[] => {
    return [
      {
        title: "Faktureringsgrad",
        values: [
          {
            value: averageFromArray(
              state.invoiceState.invoiceStatistics.invoiceRate
            ),
            unit: "%",
          },
        ],
      },
      {
        title: "Fakturerbare timer",
        values: [
          {
            value: state.invoiceState.invoiceStatistics.billableHours.reduce(
              (a, b) => a + b,
              0
            ),
          },
        ],
      },
      {
        title: "Interntimer",
        values: [
          {
            value: state.invoiceState.invoiceStatistics.nonBillableHours.reduce(
              (a, b) => a + b,
              0
            ),
          },
        ],
      },
      {
        title: "Feriedager",
        values: [
          {
            value: Math.round(
              state.invoiceState.invoiceStatistics.vacationHours.reduce(
                (a, b) => a + b,
                0
              ) / 7.5
            ),
          },
        ],
      },
	  {
		title: "Fakturerbare overtidstimer",
		values: [
		  {
			value: state.invoiceState.invoiceStatistics.overtimeHours.invoiceableOvertimeHours.reduce(
			  (a: number, b: number) => a + b,
			  0
			),
		  },
		],
	  },
	  {
		title: "Interne overtidstimer",
		values: [
		  {
			value: state.invoiceState.invoiceStatistics.overtimeHours.internalOvertimeHours.reduce(
			  (a: number, b: number) => a + b,
			  0
			),
		  },
		],
	  },
    ];
  },
};

const mutations = {
  SET_INVOICE_RATE(state: State, invoiceRate: number) {
    state.invoiceState.invoiceRate = decimalToRoundedPercentage(invoiceRate);
  },
  SET_INVOICE_STATISTIC(state: State, invoiceStatistics: InvoiceStatistics) {
    invoiceStatistics.invoiceRate = formatDecimalArray(
      invoiceStatistics.invoiceRate
    );
    invoiceStatistics.nonBillableInvoiceRate = formatDecimalArray(
      invoiceStatistics.nonBillableInvoiceRate
    );
    state.invoiceState.invoiceStatistics = invoiceStatistics;
    const granularity =
      state.invoiceState.invoiceStatisticFilters.granularity ??
      InvoicePeriods.Monthly;
    state.invoiceState.invoiceStatistics.labels = invoiceStatistics.labels.map(
      timeStamp => mapTimeStampToLabel(timeStamp, granularity)
    );
  },
  SET_INVOICE_STATISTIC_FILTERS(
    state: State,
    invoiceStatisticFilters: InvoiceStatisticsFilters
  ) {
    state.invoiceState.invoiceStatisticFilters = {
      ...state.invoiceState.invoiceStatisticFilters,
      ...invoiceStatisticFilters,
    };
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
  FETCH_INVOICE_STATISTICS: async ({
    commit,
    state,
  }: ActionContext<State, State>) => {
    const fromDate = state.invoiceState.invoiceStatisticFilters.fromDate;
    const toDate = state.invoiceState.invoiceStatisticFilters.toDate;
    const granularity =
      state.invoiceState.invoiceStatisticFilters.granularity ??
      InvoicePeriods.Monthly;

    const response = await httpClient
		  .get(
			  `${config.API_HOST}/api/user/InvoiceStatistics?fromDate=${fromDate}&toDate=${toDate}&period=${granularity}&includeZeroPeriods=${true}`
		  );
	const overtimeHours = await fetchOvertimeHours();
	  commit("SET_INVOICE_STATISTIC", { ...response.data, overtimeHours});
  },
  CHANGE_INVOICE_FILTERS: (
    { commit }: ActionContext<State, State>,
    filters: InvoiceStatisticsFilters
  ) => {
    commit("SET_INVOICE_STATISTIC_FILTERS", filters);
  },
};

function createDefaultStatisticInterval(): InvoiceStatisticsFilters {
  const to = new Date();
  const from = new Date();
  from.setMonth(from.getMonth() - 12);

  return {
    fromDate: createTimeString(from.getFullYear(), from.getMonth() + 1, 1),
    toDate: createTimeString(
      to.getFullYear(),
      to.getMonth() + 2 <= 12 ? to.getMonth() + 2 : 12,
      1
    ),
    granularity: InvoicePeriods.Monthly,
  };
}

const getInvoiceableOvertimeHours = (entries: any): number[] => {
	const { fromDate, toDate } = state.invoiceState.invoiceStatisticFilters;
	const isWithinRange = (entryDate: Date): boolean => {
	  const entry = new Date(entryDate);
	  const from = fromDate ? new Date(fromDate) : null;
	  const to = toDate ? new Date(toDate) : null;
	  return (
		(!from || entry >= from) &&
		(!to || entry <= to)
	  );
	};
	const filteredOverTimeHours = entries?.filter((entry: any) => {
		return entry.compensationRate > 1 && isWithinRange(entry.date);
	});

    return filteredOverTimeHours.map((entry: any) => entry.hours || 0);
  };

const getInternalOvertimeHours = (entries: any): number[] => {
	const { fromDate, toDate } = state.invoiceState.invoiceStatisticFilters;
	const isWithinRange = (entryDate: Date): boolean => {
	  const entry = new Date(entryDate);
	  const from = fromDate ? new Date(fromDate) : null;
	  const to = toDate ? new Date(toDate) : null;
	  return (
		(!from || entry >= from) &&
		(!to || entry <= to)
	  );
	};
	const filteredOverTimeHours = entries?.filter((entry: any) => {
		return entry.compensationRate <= 1 && isWithinRange(entry.date);
	});

    return filteredOverTimeHours.map((entry: any) => entry.hours || 0);
  };

  const fetchOvertimeHours = async () => {
    const response = await httpClient
		  .get(`${config.API_HOST}/api/user/AvailableHours`);
	  const filteredOverTimeHours = response.data.entries.filter((entry: any) => entry.type !== 1);

	  return {
		  invoiceableOvertimeHours: getInvoiceableOvertimeHours(filteredOverTimeHours),
		  internalOvertimeHours: getInternalOvertimeHours(filteredOverTimeHours),
	  };
  };

export default {
  state,
  getters,
  mutations,
  actions,
};
