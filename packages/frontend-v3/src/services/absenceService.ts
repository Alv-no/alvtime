import { api } from "@/services/apiClient";

export default {
	getAbsenceOverview: async () => await api.get("/api/user/AbsenseOverview"),
	getVacationOverview: async () => await api.get("/api/user/VacationOverview"),
};