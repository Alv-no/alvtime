import timeService from "@/services/timeService";
import { defineStore } from "pinia";
import { ref } from "vue";
import { type TimeEntry, type TimeEntryMap } from "@/types/TimeEntryTypes";
import { debounce } from "@/utils/generalHelpers";
import { useVacationStore } from "./vacationStore";
import { useTimeBankStore } from "./timeBankStore";

export type TimeEntryError = {
	status?: number;
	title?: string;
	type?: string;
	errors?: any;
};

export type InvoiceRateMonth = {
	month: number;
	rate: number;
	year: number;
};

export const useTimeEntriesStore = defineStore("timeEntries", () => {
	const timeEntries = ref<TimeEntry[]>([]);
	const timeEntriesMap = ref<TimeEntryMap>({});
	const timeEntryPushQueue = ref<TimeEntry[]>([]);
	const loadingTimeEntries = ref<boolean>(false);
	const invoiceRate = ref<InvoiceRateMonth[]>([]);
	const timeEntryError = ref<TimeEntryError>();

	const vacationStore = useVacationStore();
	const timeBankStore = useTimeBankStore();

	// Function to fetch time entries for a given date range
	const getTimeEntries = async (params: {fromDateInclusive: Date, toDateInclusive: Date}) => {
		loadingTimeEntries.value = true;
		const response = await timeService.getTimeEntries(params);
		getInvoiceRateByMonth();
		if (response.status === 200) {
			updateTimeEntries(response.data.map(createTimeEntry));
		} else {
			console.error("Failed to fetch time entries:", response.statusText);
		}
		loadingTimeEntries.value = false;
	};

	// Debounced function to push time entries to the service
	const pushTimeEntryQueue = async () => {
		timeEntryError.value = {};
		if (timeEntryPushQueue.value.length === 0) {
			return;
		}

		const timeEntriesToUpdate = [...timeEntryPushQueue.value];
		timeEntryPushQueue.value = []; // Clear the queue before processing
		try {
			const response = await timeService.updateTimeEntries(timeEntriesToUpdate);
			if (response.status === 200) {
				updateTimeEntries(response.data.map(createTimeEntry));
				timeBankStore.getTimeBankOverview();
				vacationStore.getVacationOverview();
				getInvoiceRateByMonth();
			} else {
				console.error("Failed to update time entries:", response.statusText);
			}
		} catch (error: any) {
			timeEntryError.value = error?.response?.data as TimeEntryError;
			console.error("Error updating time entries:", error);
			// TODO: This should be called to "reset" the entries that failed to be sent.
			getTimeEntries({ fromDateInclusive: new Date(), toDateInclusive: new Date() });
		}
	};

	// Debounced function to push time entries to the service with a delay
	const pushQueue = debounce(pushTimeEntryQueue, 1000);

	// Function to update or create a time entry
	const updateTimeEntry = async (timeEntry: TimeEntry) => {
		const existingEntryIndex = timeEntries.value.findIndex(
			(entry) => entry.id === timeEntry.id
		);

		if (existingEntryIndex !== -1) {
			timeEntries.value[existingEntryIndex].value = timeEntry.value;
		} else {
			timeEntries.value.push(timeEntry);
		}

		const existingQueueIndex = timeEntryPushQueue.value.findIndex(
			(entry) => entry.id === timeEntry.id
		);

		if(existingQueueIndex !== -1) {
			// If the entry already exists in the queue, update it
			timeEntryPushQueue.value[existingQueueIndex] = timeEntry;
		}
		else {
			// If the entry does not exist in the queue, add it
			timeEntryPushQueue.value.push(timeEntry);
		}

		pushQueue();
	};

	// Function to update time entries in the store
	const updateTimeEntries = async (paramEntries: TimeEntry[]) => {
		let newTimeEntriesMap = { ...timeEntriesMap.value };
		for (const paramEntry of paramEntries) {
			newTimeEntriesMap = updateTimeEntryMap(newTimeEntriesMap, paramEntry);
		}
		
		timeEntriesMap.value = { ...timeEntriesMap.value, ...newTimeEntriesMap };

		let newTimeEntries = timeEntries.value ? [...timeEntries.value] : [];
		for (const paramEntry of paramEntries) {
			newTimeEntries = updateArrayWith(newTimeEntries, paramEntry);
		}

		timeEntries.value = newTimeEntries;
	};

	// Helper function to update the time entries map with a new or existing entry
	const updateTimeEntryMap = (timeEntriesMapLocal: TimeEntryMap, timeEntry: TimeEntry): TimeEntryMap => {
		timeEntriesMapLocal[`${timeEntry.date}${timeEntry.id}`] = {
			id: timeEntry.id,
			value: timeEntry.value,
			comment: timeEntry.comment,
		};

		return timeEntriesMapLocal;
	};

	// Helper function to update the time entries array with a new or existing entry
	const updateArrayWith = (arr: TimeEntry[], paramEntry: TimeEntry ): TimeEntry[] => {
		const existingEntryIndex = arr.findIndex(
			(entry) => isMatchingEntry(entry, paramEntry)
		);

		if (existingEntryIndex !== -1) {
			return [
				...arr.map(entry =>
					isMatchingEntry(paramEntry, entry) ? paramEntry : entry
				),
			];
		} else {
			return [...arr, paramEntry];
		}
	};

	// Helper function to check if two time entries match based on id and date
	const isMatchingEntry = (entry: TimeEntry, paramEntry: TimeEntry): boolean => {
		return (entry.id === paramEntry.id) || (entry.date === paramEntry.date && entry.taskId === paramEntry.taskId);
	};

	// Function to create a time entry object with the correct date format
	const createTimeEntry = (timeEntry: TimeEntry): TimeEntry => {
		return {
			...timeEntry,
			date: timeEntry.date.split("T")[0],
			commentedAt: timeEntry.commentedAt ? new Date(timeEntry.commentedAt) : undefined,
		};
	};

	// Function to get the remaining time in the workday for a given date across all time entries.
	const getRemainingTimeInWorkday = (date: string) => {
		const entriesForDate = timeEntries.value.filter(entry => entry.date === date);

		const timeTrackedForDate = entriesForDate.reduce((total, entry) => {
			return total + (entry.value || 0);
		}, 0);

		return timeTrackedForDate > 7.5 ? 0 : 7.5 - timeTrackedForDate;
	};

	const getInvoiceRateByMonth = async (monthsToFetch:number = 6) => {
		const response = await timeService.getInvoiceRateByMonth(monthsToFetch);
		if (response.status === 200) {
			invoiceRate.value = response.data.map((item: InvoiceRateMonth) => ({
				month: item.month,
				rate: Math.round(item.rate * 1000) / 10,
				year: item.year,
			}));
		} else {
			console.error("Failed to fetch invoice rate by month:", response.statusText);
			return [];
		}
	};

	return { timeEntries, timeEntryError, invoiceRate, timeEntryPushQueue, getTimeEntries, updateTimeEntry, getRemainingTimeInWorkday };
});
