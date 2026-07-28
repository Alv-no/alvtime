<template>
	<div class="week-summary">
		<div class="icon-wrapper">
			<HugeiconsIcon
				:icon="Calendar03Icon"
				class="icon"
			/> {{ totalHoursThisWeek }}/37.5
		</div>
	</div>
</template>

<script setup lang="ts">
import { HugeiconsIcon } from "@hugeicons/vue";
import { Calendar03Icon } from "@hugeicons/core-free-icons";

const { totalHoursThisWeek } = defineProps<{
	totalHoursThisWeek: number;
}>();
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
</style>
