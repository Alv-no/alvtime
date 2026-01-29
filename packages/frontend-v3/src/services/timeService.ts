import { api } from "./apiClient";
import { formatDate } from "../utils/dateHelper";
import { type TimeEntry } from "@/types/TimeEntryTypes";

export default {
	getTimeEntries: async (params: {fromDateInclusive: Date, toDateInclusive: Date}) => await api.get(
		`/api/user/TimeEntries?fromDateInclusive=${formatDate(params.fromDateInclusive)}&toDateInclusive=${formatDate(params.toDateInclusive)}`
	),
	getInvoiceRate: async () => await api.get("/api/user/InvoiceRate"),
	getInvoiceStatistics: async (params: {fromDate: Date, toDate: Date, period: number, includeZeroPeriods: boolean}) => await api.get(
		`/api/user/InvoiceStatistics?fromDate=${formatDate(params.fromDate)}&toDate=${formatDate(params.toDate)}&period=${params.period}${params.includeZeroPeriods ? "&includeZeroPeriods=true" : ""}`
	),
	updateTimeEntries: async (timeEntries: TimeEntry[]) => await api.post(
		"/api/user/TimeEntries", timeEntries
	),
};
