<template>
	<div>
		<div class="task-strip">
			<div class="task-description">
				{{ task.name }} - {{ task.compensationRate * 100 }}%
			</div>
			<div class="task-selections">
				<div>
					<input
						v-model="task.enableComments"
						type="checkbox"
						@change="updateTask"						
					/>
				</div>
				<div>
					<input
						v-model="task.favorite"
						type="checkbox"
						@change="updateTask"
					/>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import type { Task } from "@/types/ProjectTypes";
import { useTaskStore } from "@/stores/taskStore";

const task = defineModel<Task>({ required: true });
const taskStore = useTaskStore();

const updateTask = () => {
	taskStore.updateTasks([task.value]);
};
</script>

<style lang="scss" scoped>
.task-strip {
	display: flex;
	justify-content: space-between;
	align-items: center;
	border-radius: 5px;
	font-size: 1.2rem;

	.task-selections {
		display: flex;
		gap: 8px;

		div {
			width: 100px;
			text-align: center;
		}

		input[type="checkbox"] {
			width: 20px;
			height: 20px;
			cursor: pointer;
		}
	}
}
</style>