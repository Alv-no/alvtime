<template>
	<div
		class="project-container"
	>
		<div
			class="project-container-header"
			@click="toggleExpand"
		>
			<div class="project-info-wrapper">
				<p class="project-name">
					{{ project.name }}
				</p>
				<p class="project-project">
					{{ project.customer.name }}
				</p>
			</div>
			<div class="project-status-wrapper">
				<div class="project-stats">
					Denne uken: 22,5t | Denne måneden: 145,5t
				</div>
				<FeatherIcon
					name="chevron-down"
					:size="36"
				/>
			</div>
		</div>
		<div
			class="project-container-expandable"
			:class="{ 'visible': project.open }"
		>
			<div
				class="project-container-content"
				:class="{ 'visible': project.open }"
			>
				<DayPillStrip
					:week="week"
				/>
				<TaskStrip
					v-for="task in project.tasks"
					:key="task.id"
					:task="task"
					:week="week"
				/>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import TaskStrip from "./TaskStrip.vue";
import FeatherIcon from "../utils/FeatherIcon.vue";
import { defineProps } from "vue";
import { type Project } from "@/types/ProjectTypes";
import DayPillStrip from "./DayPillStrip.vue";
import { useTaskStore } from "@/stores/taskStore";

const taskStore = useTaskStore();

const toggleExpand = () => {
	taskStore.toggleProjectExpandable(project.id);
};

const { project, week } = defineProps<{
	project: Project;
	week: Date[];
}>();
</script>

<style scoped lang="scss">
.project-container {
	border: 1px solid rgb(206, 214, 194, 0.5);
	border-radius: 10px;
}

.project-container-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 4px 16px;
	min-height: 70px;

	.project-info-wrapper {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: flex-start;
		gap: 2px;
		margin: 8px 0;
	}
	

	.project-name {
		font-size: 1.5rem;
		font-weight: 600;
		margin: 0;
	}

	.project-project {
		font-size: 1.2rem;
		font-weight: 400;
		margin: 0;
	}

	.project-status-wrapper {
		display: flex;
		justify-content: flex-end;
		align-items: center;
		gap: 16px;

		.project-stats {
			background-color: rgb(206, 214, 194);
			border-radius: 10px;
			padding: 12px 12px 9px;
		}
	}
}

.project-container-expandable {
	display: grid;
	grid-template-rows: 0fr;
	transition: grid-template-rows 200ms ease-in-out;

	&.visible {
		grid-template-rows: 1fr;
		margin-bottom: 16px;
	}
}

.project-container-content {
	display: flex;
	flex-direction: column;
	gap: 12px;
	overflow: hidden;
	padding: 0 16px;

	&.visible {
		padding-bottom: 4px;
	}
}
</style>
