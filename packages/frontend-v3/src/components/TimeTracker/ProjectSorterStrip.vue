<template>
	<div class="project-sorter-strip">
		<div>
			<p class="project-title">
				{{ project.name }}
			</p>
			<p class="customer-name">
				{{ project.customerName }}
			</p>
		</div>
		<div>
			<FeatherIcon name="menu" />
		</div>
	</div>
</template>

<script setup lang="ts">
import { defineProps } from "vue";
import type { Project } from "@/types/ProjectTypes";
import FeatherIcon from "../utils/FeatherIcon.vue";

const { project } = defineProps<{
	project: Project;
}>();
</script>

<style scoped lang="scss">
.project-sorter-strip {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 18px 16px 16px;
	border: 1px solid rgb(206, 214, 194, 0.5);
	border-radius: 15px;
	cursor: grab;

	&:not(:last-child) {
		margin-bottom: 16px;
	}

	&:active {
		background-color: darken($background-color, 5%);
	}

	p {
		margin: 0;

		&.project-title {
			font-size: 1.5rem;
			font-weight: 600;
			margin-bottom: 2px;
		}

		&.customer-name {
			font-size: 1.2rem;
			font-weight: 400;
		}
	}


}
</style>
