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

export const useStatisticsStore = defineStore("statistics", () => {
	const statistics = ref<Statistics>();

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

	return { statistics, getStatistics };
});