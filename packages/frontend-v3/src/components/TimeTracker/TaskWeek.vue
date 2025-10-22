<template>
	<div class="task-week">
		<HourInput
			v-for="timeEntry in filteredTimeEntriesForTask"
			:key="`${timeEntry.taskId}-${timeEntry.date}`"
			:timeEntry="timeEntry"
			:enableComments="task.enableComments"
		/>
	</div>
</template>

<script lang="ts" setup>
import { storeToRefs } from "pinia";
import { computed } from "vue";
import { type Task } from "@/types/ProjectTypes";
import HourInput from "./HourInput.vue";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";

const timeEntriesStore = useTimeEntriesStore();
const { timeEntries } = storeToRefs(timeEntriesStore);

const filteredTimeEntriesForTask = computed(() => {
	const entriesForTask = timeEntries.value?.filter(entry => entry.taskId === task.id);
	const allTasksForWeek = addMissingTaskEntriesToWeek(entriesForTask);
	return allTasksForWeek
		.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
});

const addMissingTaskEntriesToWeek = (entries: typeof timeEntries.value) => {
	const dateToEntry = new Map<string, typeof entries[0]>();
	entries.forEach(entry => {
		const dateStr = entry.date;
		dateToEntry.set(dateStr, entry);
	});

	const result = week.map(day => {
		const timeZonedDate = new Date(day.getTime() - day.getTimezoneOffset() * 60000);
		const dayStr = timeZonedDate.toISOString().split("T")[0];
		const existingEntry = dateToEntry.get(dayStr);
		if (existingEntry) {
			return existingEntry;
		}
		// Create a new entry object (adjust fields as needed)
		return {
			id: `new-${task.id}-${dayStr}`,
			taskId: task.id,
			date: dayStr,
			value: 0,
			locked: false,
			// Add other required fields with default values if needed
		};
	});

	return result;
};

const { task, week } = defineProps<{
	task: Task;
	week: Date[];
}>();
</script>

<style lang="scss" scoped>
.task-week {
	display: flex;
	flex-direction: row;
	align-items: center;
	gap: 1rem;

	@media screen and (max-width: 768px) {
		gap: unset;
		justify-content: space-between;
	}
}
</style>
