import { api } from "./apiClient";
import { formatDate } from "../utils/dateHelper";
import { type TimeEntry } from "@/types/TimeEntryTypes";

export default {
	getTimeEntries: async (fromDateInclusive: Date, toDateInclusive: Date) => await api.get(
		`/api/user/TimeEntries?fromDateInclusive=${formatDate(fromDateInclusive)}&toDateInclusive=${formatDate(toDateInclusive)}`
	),
	updateTimeEntries: async (timeEntries: TimeEntry[]) => await api.post(
		"/api/user/TimeEntries", timeEntries
	),
};
