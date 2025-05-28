import { api } from "./apiClient";
import { formatDate } from "../utils/dateHelper";

export default {
	getTimeEntries: async (fromDateInclusive: Date, toDateInclusive: Date) => await api.get(
		`/api/user/TimeEntries?fromDateInclusive=${formatDate(fromDateInclusive)}&toDateInclusive=${formatDate(toDateInclusive)}`
	),
};