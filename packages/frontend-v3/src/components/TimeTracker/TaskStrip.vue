<template>
	<div class="task-strip">
		<div class="task-description">
			{{ task.name }} - {{ compensationRateInPercentage }}
		</div>
		<TaskWeek
			class="task-week"
			:task="task"
			:week="week"
			:lockedTo="lockedTo"
		/>
	</div>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { type Task } from "@/types/ProjectTypes";
import TaskWeek from "./TaskWeek.vue";

const { task, week, lockedTo } = defineProps<{
  task: Task;
  week: Date[];
  lockedTo: Date | null;
}>();

const compensationRateInPercentage = computed(() => {
  	return `${task.compensationRate * 100}%`;
});
</script>

<style lang="scss" scoped>
.task-strip {
	display: flex;
	position: relative;
	justify-content: space-between;
	align-items: baseline;
	font-size: 1.2rem;
	flex-wrap: wrap;
	text-box: trim-both cap alphabetic;
	
	.task-description {
		text-box: trim-both cap alphabetic;
	}

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

	&::after {
		content: '';
		position: absolute;
		inset: -5px -10px; // -vertikal -horisontal (topp/bunn -10px, venstre/høyre -15px)
		border-style: solid;
		border-width: 5px 10px; // topp/bunn 10px, venstre/høyre 15px
		border-color: transparent;
		box-sizing: border-box;
		pointer-events: none; // så pseudo-elementet ikke stjeler hover/klikk
		border-radius: 10px;
	}

	&:hover {
		background-color: rgb(230, 192, 141);

		&::after {
		border-color: rgb(230, 192, 141);
		}
	}
}
</style>
