<template>
	<div class="project-container">
		<div class="project-info-wrapper">
			<p class="project-name">
				{{ project.name }}
			</p>
			<p class="project-project">
				{{ project.customer.name}}
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
</template>

<script setup lang="ts">
import FeatherIcon from "../utils/FeatherIcon.vue";
import { defineProps } from "vue";

interface Customer {
	name: string;
}

interface Project {
	name: string;
	customer: Customer;
}

const { project } = defineProps<{
	project: Project;
}>();
</script>

<style scoped lang="scss">
.project-container {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 4px 16px;
	margin-top: 16px;
	border: 1px solid rgb(206, 214, 194, 0.5);
	border-radius: 10px;
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
</style>