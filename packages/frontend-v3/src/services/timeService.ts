import { api } from "./apiClient";
import { formatDate } from "../utils/dateHelper";
import { type TimeEntry } from "@/types/TimeEntryTypes";

export default {
	getTimeEntries: async (params: {fromDateInclusive: Date, toDateInclusive: Date}) => await api.get(
		`/api/user/TimeEntries?fromDateInclusive=${formatDate(params.fromDateInclusive)}&toDateInclusive=${formatDate(params.toDateInclusive)}`
	),
	getInvoiceRate: async () => await api.get("/api/user/InvoiceRate"),
	updateTimeEntries: async (timeEntries: TimeEntry[]) => await api.post(
		"/api/user/TimeEntries", timeEntries
	),
};
