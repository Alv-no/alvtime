import { defineStore } from "pinia";
import { ref } from "vue";
import absenceService from "@/services/absenceService";

type Vacation = {
	availableVacationDays: number;
	availableVacationDaysTransferredFromLastYear: number;
	usedVacationDaysThisYear: number;
	plannedVacationDaysThisYear: number;
	plannedTransactions: Transaction[];
	usedTransactions: Transaction[];
};

type Transaction = {
	comment: string;
	commentedAt: string;
	date: string;
	id: number;
	taskId: number;
	user: number;
	userEmail: string;
	value: number;
};

export const useVacationStore = defineStore("vacation", () => {
	const vacation = ref<Vacation>();

	const getVacationOverview = async () => {
		try {
			const response = await absenceService.getVacationOverview();
			vacation.value = response.data;
		} catch (error) {
			console.error("Failed to fetch vacation overview:", error);
			throw error;
		}
	};

	return { vacation, getVacationOverview };
});
