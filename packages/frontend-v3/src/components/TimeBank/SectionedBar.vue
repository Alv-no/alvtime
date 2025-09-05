<template>
	<div class="sectioned-bar-titles">
		<div
			v-for="(section, index) in filteredSections"
			:key="index"
			class="title"
			:style="{ width: `${(section.amount / total) * 100}%` }"
		>
			{{ section.title }}
		</div>
	</div>
	<div class="sectioned-bar">
		<div
			v-for="(section, index) in filteredSections"
			:key="index"
			class="bar"
			:class="section.color"
			:style="{ width: `${(section.amount / total) * 100}%` }"
		>
			{{ section.amount }}
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed } from "vue";

type Section = {
	title: string;
	amount: number;
	color: string;
};

const { sections } = defineProps<{
	sections: Section[];
}>();

const filteredSections = computed(() => sections.filter(section => section.amount > 0));

const total = computed(() => filteredSections.value.reduce((acc, section) => acc + section.amount, 0));
</script>

<style scoped lang="scss">
.sectioned-bar-titles {
	display: flex;
	flex-direction: row;
	justify-content: space-between;
	margin-bottom: 5px;

	.title {
		text-align: center;
	}
}
.sectioned-bar {
	border-radius: 25px;
	overflow: hidden;
	display: flex;
	flex-direction: row;

	.bar {
		height: 35px;
		text-align: center;
		align-content: center;
		padding-top: 4px;
		padding-bottom: 2px;
		font-weight: 700;
	}

	.yellow {
		background-color: $accent-color;
	}

	.green {
		background-color: $secondary-color;
	}

	.blue {
		background-color: $primary-color;
		color: $background-color;
	}

	.red {
		background-color: #721c24;
		color: $background-color
	}
}
</style>