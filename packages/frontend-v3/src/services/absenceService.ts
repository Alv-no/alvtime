import { api } from "@/services/apiClient";

export default {
	getAbsenceOverview: async () => await api.get("/api/user/AbsenceOverview"),
	getVacationOverview: async () => await api.get("/api/user/VacationOverview"),
};