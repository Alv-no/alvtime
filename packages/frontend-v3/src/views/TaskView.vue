<template>
	<div class="secret-view">
		<h1>Aktiviteter</h1>
		<div
			v-if="!loading"
			class="project-list-wrapper"
		>
			<FilterInput />
			<ProjectExpandable
				v-for="project in taskStore.filteredProjects"
				:key="project.id"
				:project="project"
			>
				<template #content>
					<ActivityStripHeader />
					<ul>
						<li
							v-for="task in project.tasks"
							:key="task.id"
						>
							<ActivityStrip
								:modelValue="task"
							/>
						</li>
					</ul>
				</template>
			</ProjectExpandable>
		</div>
	</div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import ProjectExpandable from "@/components/TimeTracker/ProjectExpandable.vue";
import { useTaskStore } from "@/stores/taskStore";
import ActivityStrip from "@/components/Tasks/ActivityStrip.vue";
import ActivityStripHeader from "@/components/Tasks/ActivityStripHeader.vue";
import FilterInput from "@/components/Tasks/FilterInput.vue";

const loading = ref(true);
const taskStore = useTaskStore();

onMounted(async () => {
	await taskStore.getTasks();
	loading.value = false;
});
</script>

<style lang="scss" scoped>
.project-list-wrapper {
	display: flex;
	flex-direction: column;
	gap: 16px;
	margin-top: 16px;

	ul {
		list-style-type: none;
		padding: 0;
		display: flex;
		flex-direction: column;	
		gap: 8px;
		margin: 0;
	}
}
</style>