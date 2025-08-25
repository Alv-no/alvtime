import { defineStore } from "pinia";
import { ref } from "vue";
import { timeBankService } from "@/services/timeBankService";

type TimeBankTransaction = {
	availableHoursBeforeCompensation: number;
	availableHoursAfterCompensation: number;
	entries: TimeBankEntry[];
};

type TimeBankEntry = {
	compensationRate: number;
	date: string;
	hours: number;
	type: TransactionType;
};

const TransactionType = {
	Overtime: 0,
	Payout: 1,
	Flex: 2,
} as const;
type TransactionType = typeof TransactionType[keyof typeof TransactionType];

export const useTimeBankStore = defineStore("timeBank", () => {
	const timeBankOverview = ref<TimeBankTransaction>();

	const getTimeBankOverview = async () => {
		try {
			const response = await timeBankService.getTimeBankOverview();
			timeBankOverview.value = response.data;
		} catch (error) {
			console.error("Failed to fetch time bank overview:", error);
		}
	};

	return {
		timeBankOverview,
		getTimeBankOverview,
	};
});
