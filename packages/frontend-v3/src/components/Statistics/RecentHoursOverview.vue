<template>
	<div class="table-container">
		<h3>Timesoversikt siste 3 måneder</h3>
		<table class="time-overview-table">
			<thead>
				<tr>
					<th>Timekode</th>
					<th
						v-for="month in sortedMonths"
						:key="`${month.year}-${month.month}`"
					>
						{{ formatMonth(month.year, month.month) }}
					</th>
				</tr>
			</thead>
			<tbody>
				<tr
					v-for="taskId in allTaskIds"
					:key="taskId"
				>
					<td>{{ getTaskName(taskId) }}</td>
					<td
						v-for="month in sortedMonths"
						:key="`${month.year}-${month.month}-${taskId}`"
					>
						{{ getHoursForTask(taskId, month.year, month.month) || '-' }}
					</td>
				</tr>
			</tbody>
		</table>
	</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { TimeEntryOverview } from "@/stores/statisticsStore.ts";
import type { Project } from "@/types/ProjectTypes.ts";

const props = defineProps<{
  timeEntryOverview: TimeEntryOverview[],
  projects: Project[]
}>();

const sortedMonths = computed(() => {
	return [...props.timeEntryOverview].sort((a, b) => {
		if (a.year !== b.year) return a.year - b.year;
		return a.month - b.month;
	});
});

const allTaskIds = computed(() => {
	const taskIds = new Set<number>();
	props.timeEntryOverview.forEach(entry => {
		entry.tasksWithHours.forEach(task => taskIds.add(task.taskId));
	});
	return Array.from(taskIds);
});

function getTaskName(taskId: number): string {
	for (const project of props.projects) {
		const task = project.tasks.find(t => Number(t.id) === taskId);
		if (task) return task.name;
	}
	return `Timekode ${taskId}`;
}

function getHoursForTask(taskId: number, year: number, month: number): number | null {
	const monthData = props.timeEntryOverview.find(
		entry => entry.year === year && entry.month === month
	);
	if (!monthData) return null;

	const taskData = monthData.tasksWithHours.find(t => t.taskId === taskId);
	return taskData?.hours ?? null;
}

function formatMonth(year: number, month: number): string {
	const date = new Date(year, month - 1);
	const formatted = date.toLocaleDateString("nb-NO", { month: "short", year: "numeric" });
	return formatted.charAt(0).toUpperCase() + formatted.slice(1);
}
</script>

<style scoped lang="scss">
.table-container {
  padding: 0.2rem 0.5rem 1.0rem 0.5rem;
  display: flex;
  flex-direction: column;
  border: 3px solid $secondary-color;
  border-radius: 0.5rem;
  margin-bottom: 0.5rem;
}

.time-overview-table {
  width: 100%;
  border-collapse: collapse;

  th, td {
    padding: 0.75rem;
    border-bottom: 1px solid #e0e0e0;
  }

  th {
    font-weight: bold;
    background-color: $secondary-color-light;
  }

  td {
    text-align: center;
  }
}
</style>