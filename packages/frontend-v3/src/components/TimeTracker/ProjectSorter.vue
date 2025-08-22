<template>
	<div class="project-sorter">
		<div class="project-sorter-content">
			<Sortable
				:list="taskStore.favoriteProjects"
				itemKey="name"
				:options="options"
			>
				<template #item="{ element }">
					<ProjectSorterStrip
						:project="element"
					/>
				</template>
			</Sortable>
		</div>
	</div>
</template>

<script setup lang="ts">
import { useTaskStore } from "@/stores/taskStore";
import { Sortable } from "sortablejs-vue3";
import ProjectSorterStrip from "./ProjectSorterStrip.vue";

const taskStore = useTaskStore();
const options = {
	animation: 150,
};
</script>

<style scoped lang="scss">
	.project-sorter {
		position: fixed;
		width: 100%;
		height: 100%;
		background-color: rgba(0, 0, 0, 0.5);
		top: 0;
		left: 0;
		z-index: 100;
		display: flex;
		justify-content: center;
		align-items: center;

		.project-sorter-content {
			background-color: $background-color;
			border-radius: 25px;
			min-width: 500px;
			padding: 16px;
		}
	}
</style>
