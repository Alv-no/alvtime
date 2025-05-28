import timeService from "@/services/timeService";
import { defineStore } from "pinia";
import { ref } from "vue";

interface TimeEntry {
	// Define the properties of a time entry according to your data structure
	id: number;
	date: string;
	hours: number;
	// Add other relevant fields here
}

export const useTimeEntriesStore = defineStore("timeEntries", () => {
	const timeEntries = ref<TimeEntry[] | null>(null);

	const getTimeEntries = async (startDate: Date, endDate: Date) => {
		const response = await timeService.getTimeEntries(startDate, endDate);
		if (response.status === 200) {
			console.log("Time entries fetched successfully:", response.data);
			timeEntries.value = response.data;
		} else {
			console.error("Failed to fetch time entries:", response.statusText);
			timeEntries.value = [];
		}
	};

	return { timeEntries, getTimeEntries };
});
