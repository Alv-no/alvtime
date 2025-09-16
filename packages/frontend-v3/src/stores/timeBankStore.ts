/* eslint-disable @typescript-eslint/no-explicit-any */
import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { timeBankService } from "@/services/timeBankService";
import { setLocalSalary, getLocalSalary } from "@/composables/useLocalStorage";

type TimeBankTransaction = {
	availableHoursBeforeCompensation: number;
	availableHoursAfterCompensation: number;
	entries: TimeBankEntry[];
};

export type TimeBankEntry = {
	compensationRate: number;
	date: string;
	hours: number;
	type: TransactionType;
	active?: boolean;
};

const TransactionType = {
	Overtime: 0,
	Payout: 1,
	Flex: 2,
} as const;
type TransactionType = typeof TransactionType[keyof typeof TransactionType];

export type TimeBankError = {
	status?: number;
	title?: string;
	type?: string;
	errors?: any;
};

export const useTimeBankStore = defineStore("timeBank", () => {
	const timeBankOverview = ref<TimeBankTransaction>();
	const timeBankError = ref<TimeBankError>({});
	const timeBankSalary = ref<number | null>(null);

	const getTimeBankOverview = async () => {
		timeBankError.value = {};

		try {
			const response = await timeBankService.getTimeBankOverview();
			timeBankOverview.value = response.data;
			getTimeBankSalary();
		} catch (error: any) {
			timeBankError.value = error?.response?.data as TimeBankError;
			console.error("Failed to fetch time bank overview:", timeBankError.value.errors);
		}
	};

	const setTimeBankSalary = (salary: number | null) => {
		timeBankSalary.value = salary;
		setLocalSalary(salary);
	};

	const getTimeBankSalary = () => {
		if (timeBankSalary.value === null) {
			const localSalary = getLocalSalary();
			if (localSalary !== null) {
				timeBankSalary.value = localSalary;
			}
		}
	};

	const timeBankHistory = computed(() => {
		const entries = timeBankOverview.value?.entries.filter(entry => entry.hours !== 0) || [];
		const history: { date: string; type: TransactionType; entries: TimeBankEntry[] }[] = [];

		const grouped: Record<string, Record<TransactionType, TimeBankEntry[]>> = {};

		entries.forEach(entry => {
			if (!grouped[entry.date]) {
				grouped[entry.date] = {} as Record<TransactionType, TimeBankEntry[]>;
			}
			if (!grouped[entry.date][entry.type]) {
				grouped[entry.date][entry.type] = [];
			}
			grouped[entry.date][entry.type].push(entry);
		});

		Object.entries(grouped).forEach(([date, types]) => {
			Object.entries(types).forEach(([type, entries]) => {
				history.push({
					date,
					type: Number(type) as TransactionType,
					entries,
				});
			});
		});

		return history.sort((a, b) => {
			return new Date(b.date).getTime() - new Date(a.date).getTime();
		});
	});

	const orderTimeBankPayout = async (hours: number) => {
		timeBankError.value = {};

		try {
			await timeBankService.orderTimeBankPayout({ hours, date: new Date().toISOString().split("T")[0] });
			getTimeBankOverview();
		} catch (error: any) {
			timeBankError.value = error?.response?.data as TimeBankError;
			console.error("Failed to order time bank payout:", timeBankError.value.errors);
		}
	};

	const cancelTimeBankPayout = async (date: string) => {
		timeBankError.value = {};

		try {
			await timeBankService.deleteTimeBankPayout(date);
			getTimeBankOverview();
		} catch (error: any) {
			timeBankError.value = error?.response?.data as TimeBankError;
			console.error("Failed to cancel time bank payout:", timeBankError.value.errors);
		}
	};

	return {
		timeBankOverview,
		timeBankError,
		getTimeBankOverview,
		timeBankHistory,
		timeBankSalary,
		setTimeBankSalary,
		orderTimeBankPayout,
		cancelTimeBankPayout,
	};
});
