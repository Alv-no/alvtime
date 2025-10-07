<template>
	<div class="task-strip">
		<div class="task-description">
			{{ task.name }} - {{ compensationRateInPercentage }}
		</div>
		<TaskWeek
			class="task-week"
			:task="task"
			:week="week"
		/>
	</div>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { type Task } from "@/types/ProjectTypes";
import TaskWeek from "./TaskWeek.vue";

const { task, week } = defineProps<{
  task: Task;
  week: Date[];
}>();

const compensationRateInPercentage = computed(() => {
  	return `${task.compensationRate * 100}%`;
});
</script>

<style lang="scss" scoped>
.task-strip {
	display: flex;
	justify-content: space-between;
	align-items: center;
	border-radius: 5px;
	font-size: 1.2rem;
	flex-wrap: wrap;

	@media screen and (max-width: 768px) {
		.task-description {
			font-size: 1rem;
			font-weight: 600;
			margin-top: .5rem;
		}
		.task-week {
			width: 100%;
			margin-top: 0.5rem;
		}
	}
}
</style>
