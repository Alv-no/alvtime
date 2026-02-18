import { defineStore } from "pinia";
import { ref } from "vue";
import timeService from "@/services/timeService.ts";

export type Statistics = {
  billableHours: number[];
  nonBillableHours: number[];
  vacationHours: number[];
  invoiceRate: number[];
  nonBillableInvoiceRate: number[];
	start: Date[];
	end: Date[];
}

export type TimeEntryOverview = {
	year: number;
	month: number;
	tasksWithHours: TaskWithHours[];
}

export type TaskWithHours = {
	taskId: number;
	hours: number;
}

export const useStatisticsStore = defineStore("statistics", () => {
	const statistics = ref<Statistics>();
	const timeEntryOverview = ref<TimeEntryOverview[]>();

	type GetStatisticsParams = {
		fromDate: Date;
		toDate: Date;
		period?: number;
		includeZeroPeriods?: boolean;
	};

	const getStatistics = async (params: GetStatisticsParams) => {
		try {
			const response = await timeService.getInvoiceStatistics({
				fromDate: params.fromDate,
				toDate: params.toDate,
				period: params.period ?? 2,
				includeZeroPeriods: params.includeZeroPeriods ?? false,
			});

			statistics.value = response.data;
		} catch (error) {
			console.error("Failed to fetch statistics:", error);
			throw error;
		}
	};

	const getTimeEntryOverview = async () => {
		try {
			const response = await timeService.getTimeEntriesOverview();
			timeEntryOverview.value = response.data;
		} catch (error) {
			console.error("Failed to fetch time entry overview:", error);
			throw error;
		}
	};

	return { statistics, timeEntryOverview, getTimeEntryOverview, getStatistics };
});