import axios from 'axios';
import config from "@/config";

const api = axios.create({
	baseURL: config.API_HOST,
	headers: {
		'Content-Type': 'application/json',
	}
});

export default {
	getAbsenceOverview: async () => await api.get("/api/user/AbsenceOverview"),
	getVacationOverview: async () => await api.get("/api/user/VacationOverview"),
};