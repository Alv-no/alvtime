<template>
	<div
		class="project-stats"
	>
		Denne uken: {{ allHoursInProjectThisWeek }} | Denne måneden: {{ allHoursInProjectThisMonth }}
	</div>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import { storeToRefs } from "pinia";
import type { Project } from "@/types/ProjectTypes";
import { adjustTimeOfDay } from "@/utils/dateHelper";

const timeEntriesStore = useTimeEntriesStore();

const { timeEntries } = storeToRefs(timeEntriesStore);

const { project, currentWeek } = defineProps<{
	project: Project;
	currentWeek: Date[];
}>();

const allHoursInProjectThisWeek = computed(() => {
	const taskIds = project.tasks.map((task) => task.id);
	const filteredTimeEntries = timeEntries.value.filter((entry) =>
		taskIds.includes(entry.taskId)
	);

	const totalHoursProjectThisWeek = filteredTimeEntries.reduce((total, entry) => {
		const entryDate = new Date(entry.date);

		if (entryDate >= currentWeek[0] && entryDate <= adjustTimeOfDay(currentWeek[6])) {
			return total + entry.value;
		}
		return total;
	}, 0);

	return `${totalHoursProjectThisWeek}t`;
});

const allHoursInProjectThisMonth = computed(() => {
	const taskIds = project.tasks.map((task) => task.id);
	const filteredTimeEntries = timeEntries.value.filter((entry) =>
		taskIds.includes(entry.taskId)
	);

	const totalHoursProjectThisMonth = filteredTimeEntries.reduce((total, entry) => {
		if (entry.date.includes(`${currentWeek[0].getFullYear().toString()}-${(currentWeek[0].getMonth() + 1).toString().padStart(2, "0")}`)) {
			return total + entry.value;
		}
		return total;
	}, 0);

	return `${totalHoursProjectThisMonth}t`;
});
</script>

<style lang="scss" scoped>
.project-stats {
	background-color: rgb(206, 214, 194);
	border-radius: 10px;
	padding: 12px 12px 9px;
}
</style>