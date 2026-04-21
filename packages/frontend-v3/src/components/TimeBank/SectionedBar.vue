<template>
	<div class="sectioned-bar-wrapper">
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
				@mouseenter="onBarEnter($event, index)"
				@mouseleave="hoveredIndex = null"
			>
				{{ section.amount }}
			</div>
		</div>
		<div
			v-if="hoveredIndex !== null && hoveredSection?.dates?.length"
			class="bar-tooltip"
			:style="{ left: tooltipLeft + 'px' }"
		>
			<div v-for="date in hoveredSection.dates" :key="date">{{ formatDate(date) }}</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";

type Section = {
	title: string;
	amount: number;
	color: string;
	dates?: string[];
};

const { sections } = defineProps<{
	sections: Section[];
}>();

const filteredSections = computed(() => sections.filter(section => section.amount > 0));
const total = computed(() => filteredSections.value.reduce((acc, section) => acc + section.amount, 0));

const hoveredIndex = ref<number | null>(null);
const tooltipLeft = ref(0);

const hoveredSection = computed(() =>
	hoveredIndex.value !== null ? filteredSections.value[hoveredIndex.value] : null
);

function onBarEnter(event: MouseEvent, index: number) {
	hoveredIndex.value = index;
	const el = event.currentTarget as HTMLElement;
	tooltipLeft.value = el.offsetLeft + el.offsetWidth / 2;
}

function formatDate(date: string): string {
	return new Date(date).toLocaleDateString("nb-NO", { day: "numeric", month: "long" });
}
</script>

<style scoped lang="scss">
.sectioned-bar-wrapper {
	position: relative;
}

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
		cursor: default;
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

	.hatched-green {
		background:
      repeating-linear-gradient(
			45deg,
			transparent, transparent 5px,
      $secondary-color 6px, $secondary-color 6px
    ),
      repeating-linear-gradient(
      -45deg,
      transparent, transparent 5px,
      $secondary-color 6px, $secondary-color 6px
    ),
      $secondary-color-light;
	}
}

.bar-tooltip {
	position: absolute;
	top: calc(100% + 6px);
	transform: translateX(-50%);
	background-color: $primary-color;
	color: $background-color;
	border-radius: 8px;
	padding: 8px 12px;
	z-index: 10;
	white-space: nowrap;
	pointer-events: none;
	font-size: 0.85rem;
	line-height: 1.6;
	box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}
</style>
