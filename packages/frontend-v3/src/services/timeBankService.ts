import { api } from "./apiClient";

const timeBankService = {
	getTimeBankOverview: async () => {
		return await api.get("/api/user/AvailableHours");
	},
	orderTimeBankPayout: async (data: { hours: number; date: string; }) => {
		return await api.post("/api/user/Payouts", data);
	},
	deleteTimeBankPayout: async (payoutDate: string) => {
		return await api.delete(`/api/user/Payouts?payoutDate=${payoutDate}`);
	},
};

export { timeBankService };
