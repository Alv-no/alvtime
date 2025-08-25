<template>
	<div
		class="project-sorter"
		@click.self="closeSorter"
	>
		<div class="project-sorter-content">
			<div class="project-sorter-header">
				<h2>Sorter prosjekter</h2>
				<button>
					<FeatherIcon
						name="x"
						:size="24"
						@click="closeSorter"
					/>
				</button>
			</div>
			<Sortable
				:list="favoriteProjects"
				itemKey="name"
				:options="options"
				@end="updateList"
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
import { storeToRefs } from "pinia";
import FeatherIcon from "@/components/utils/FeatherIcon.vue";

const taskStore = useTaskStore();
const { favoriteProjects } = storeToRefs(taskStore);
const options = {
	animation: 150,
};

const closeSorter = () => {
	taskStore.editingProjectOrder = false;
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const updateList = (evt: any) => {
	const moveItemInArray = <T>(array: T[], from: number, to: number) => {
		const item = array.splice(from, 1)[0];
		array.splice(to, 0, item);
	};

	const { oldIndex, newIndex } = evt;
	moveItemInArray(favoriteProjects.value, oldIndex, newIndex);
	taskStore.setFavoriteProjectsOrder();
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

			.project-sorter-header {
				display: flex;
				justify-content: space-between;
				align-items: center;
				margin-bottom: 16px;
		
				button {
					border-radius: 5px;
					outline: none;
					background-color: $secondary-color;
					color: $primary-color;
					padding: 4px 6px;
					border: none;
		
					&:hover {
						background-color: $secondary-color-light;
					}
		
					&:active {
						background-color: $secondary-color;
					}
				}
			}
		}
	}
</style>
