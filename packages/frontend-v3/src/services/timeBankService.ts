import { api } from "./apiClient";

const timeBankService = {
	getTimeBankOverview: async () => {
		return await api.get("/api/user/AvailableHours");
	},
};

export { timeBankService };
