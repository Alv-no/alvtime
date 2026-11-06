<template>
	<div class="absence-plan-skipped">
		<template v-if="skippedDays.length > 0">
			<h3>Hoppes over ({{ skippedDays.length }})</h3>
			<ul>
				<li
					v-for="day in skippedDays"
					:key="day.date"
				>
					<span class="day-label">{{ formatDayLabel(day.date) }}</span>
					<span>{{ describeDay(day) }}</span>
				</li>
			</ul>
		</template>
		<p class="non-working-days">
			{{ nonWorkingDaysLabel }}
		</p>
	</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { AbsencePlanDay } from "@/types/AbsencePlanTypes";
import { describeDay, formatDayLabel } from "@/utils/absencePlanHelper";

const { days } = defineProps<{
	days: AbsencePlanDay[];
}>();

const skippedDays = computed(() =>
	days.filter(d => d.status === "AlreadyRegistered" || d.status === "HasOtherHours" || d.status === "Blocked")
);

const nonWorkingDaysLabel = computed(() => {
	const weekendCount = days.filter(d => d.status === "Weekend").length;
	const redDayCount = days.filter(d => d.status === "RedDay").length;
	const weekend = weekendCount === 1 ? "1 helgedag" : `${weekendCount} helgedager`;
	const redDays = redDayCount === 1 ? "1 rød dag" : `${redDayCount} røde dager`;
	return `I tillegg hoppes ${weekend} og ${redDays} over.`;
});
</script>

<style scoped lang="scss">
.absence-plan-skipped {
	h3 {
		font-size: 1rem;
		margin: 12px 0 4px;
	}

	ul {
		list-style: none;
		padding: 0;
		margin: 0;
	}

	li {
		display: flex;
		gap: 12px;
		padding: 2px 0;
	}

	.day-label {
		min-width: 110px;
		font-variant-numeric: tabular-nums;
	}

	.non-working-days {
		margin: 8px 0 0;
		opacity: 0.75;
	}
}
</style>
