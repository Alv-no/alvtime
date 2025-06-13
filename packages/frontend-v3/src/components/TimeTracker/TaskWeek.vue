<template>
	<div class="task-week">
		<HourInput
			v-for="timeEntry in filteredAndSortedTimeEntries"
			:key="`${timeEntry.taskId}-${timeEntry.date}`"
			:timeEntry="timeEntry"
		/>
	</div>
</template>

<script lang="ts" setup>
import { defineProps, computed } from "vue";
import { type Task } from "@/types/ProjectTypes";
import HourInput from "./HourInput.vue";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";

const timeEntriesStore = useTimeEntriesStore();
const { timeEntries } = timeEntriesStore;

const filteredAndSortedTimeEntries = computed(() => {
	const entriesForTask = timeEntries.filter((entry) => entry.taskId === task.id);

	// Map dates to existing entries for quick lookup
	const dateToEntry = new Map<string, typeof entriesForTask[0]>();
	entriesForTask.forEach(entry => {
		const dateStr = entry.date;
		dateToEntry.set(dateStr, entry);
	});

	// For each day in the week, ensure there is an entry (existing or new)
	const result = week.map(day => {
		const dayStr = day.toISOString().split("T")[0];
		const existingEntry = dateToEntry.get(dayStr);
		if (existingEntry) {
			return existingEntry;
		}
		// Create a new entry object (adjust fields as needed)
		return {
			taskId: task.id,
			date: dayStr,
			value: 0,
			locked: false,			
			// Add other required fields with default values if needed
		};
	});

	return result.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
});

const { task, week } = defineProps<{
	task: Task;
	week: Date[];
}>();
</script>

<style lang="scss" scoped>
.task-week {
	display: flex;
	flex-direction: row;
	gap: 1rem;
}
</style>
