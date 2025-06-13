<template>
	<div class="task-strip">
		<div class="task-description">
			{{ task.name }} - {{ compensationRateInPercentage }}
		</div>
		<TaskWeek
			:task="task"
			:week="week"
		/>
	</div>
</template>

<script lang="ts" setup>
import { defineProps, computed } from "vue";
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
}
</style>
