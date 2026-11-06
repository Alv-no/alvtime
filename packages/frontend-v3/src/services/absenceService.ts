import { api } from "@/services/apiClient";
import { formatDate } from "@/utils/dateHelper";
import type { AbsencePlanParams } from "@/types/AbsencePlanTypes";

export default {
	getAbsenceOverview: async () => await api.get("/api/user/AbsenseOverview"),
	getVacationOverview: async () => await api.get("/api/user/VacationOverview"),
	getAbsencePlanTasks: async () => await api.get("/api/user/AbsencePlan/Tasks"),
	getAbsencePlan: async (params: AbsencePlanParams) => await api.get("/api/user/AbsencePlan", {
		params: {
			taskId: params.taskId,
			fromDateInclusive: formatDate(params.fromDateInclusive),
			toDateInclusive: formatDate(params.toDateInclusive),
			percentage: params.percentage,
			fillPartialDays: params.fillPartialDays,
		},
	}),
	registerAbsencePlan: async (params: AbsencePlanParams) => await api.post("/api/user/AbsencePlan", {
		...params,
		fromDateInclusive: formatDate(params.fromDateInclusive),
		toDateInclusive: formatDate(params.toDateInclusive),
	}),
};
