<template>
	<div
		class="week-summary"
		@mouseenter="hovering = true"
		@mouseleave="hovering = false"
	>
		<div class="icon-wrapper">
			<HugeiconsIcon
				:icon="Calendar03Icon"
				class="icon"
			/> {{ totalHoursThisWeek }}/37.5
		</div>
		<div
			id="expander"
			class="expander"
			:class="{ visible: hovering }"
		>
			<div
				v-for="day in totalHoursEachDayThisWeek"
				:key="day.date.toISOString()"
			>
				<span class="bold">{{ day.date.toLocaleDateString("nb-No", { weekday: "short" }).charAt(0).toUpperCase() }}</span>: {{ day.hours }}
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { HugeiconsIcon } from "@hugeicons/vue";
import { Calendar03Icon } from "@hugeicons/core-free-icons";

const hovering = ref(false);

const { totalHoursThisWeek, totalHoursEachDayThisWeek } = defineProps<{
	totalHoursThisWeek: number;
	totalHoursEachDayThisWeek: { date: Date, hours: number }[];
}>();

const toggleExpander = () => {
	const expander = document.getElementById("expander");
	const isMobile = window.innerWidth < 768;
	
	if (hovering.value && !isMobile) {
		expander?.classList.add("visible");
		expander?.style.setProperty("width", expander?.scrollWidth + "px");
	} else {
		expander?.classList.remove("visible");
		expander?.style.setProperty("width", "0");
	}
};

watch(hovering, () => {
	toggleExpander();
});
</script>

<style scoped lang="scss">
.week-summary {
	display: flex;
	align-items: center;
	gap: 10px;
}
.icon-wrapper {
	display: flex;
	align-items: center;
	gap: 5px;
	width: max-content;
}
.icon {
	position: relative;
	top: -2px;
}

.expander {
	display: flex;
	flex-direction: row;
	align-items: center;
	justify-content: flex-start;
	gap: 10px;
	width: 0;
	overflow: hidden;
	transition: width .5s ease;

	div {
		flex-grow: 1;
		white-space: nowrap;

		.bold {
			font-weight: 700;
		}
	}
}
</style>