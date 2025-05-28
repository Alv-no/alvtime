<template>
	<div class="time-tracker-container">
		<div class="time-tracker-header">
			<div class="week-number-container">
				<!-- TODO: Fiks ukenummer -->
				UKE 21
			</div>
		</div>
		<div class="project-list-wrapper">
			<ProjectExpandable v-for="project in taskStore.tasks" :project="project" :key="project.id" />
		</div>
	</div>
</template>

<script setup lang="ts">
import ProjectExpandable from "./ProjectExpandable.vue";
import { useTaskStore } from "@/stores/taskStore";

const taskStore = useTaskStore();
</script>

<style scoped lang="scss">
.time-tracker-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.week-number-container {
	background-color: rgb(230, 192, 141);
	border-radius: 10px;
	padding: 12px 12px 9px;
	font-weight: 600;
	font-size: 14px;
}

.project-list-wrapper {
	display: flex;
	flex-direction: column;
	gap: 16px;
	margin-top: 16px;
}
</style>
