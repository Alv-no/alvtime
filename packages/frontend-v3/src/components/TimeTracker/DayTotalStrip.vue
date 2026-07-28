<template>
	<div class="day-total-strip">
		<div
			v-for="day in dayTotals"
			:key="day.dateString"
			class="day-total"
			:class="{ today: day.today, zero: day.hours === 0 }"
		>
			<span class="day-label">{{ day.label }}</span>
			<span class="day-hours">{{ day.hoursString }}</span>
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import { dayOfWeek, formatDate, isSameDay } from "@/utils/dateHelper";

const timeEntriesStore = useTimeEntriesStore();

const { week } = defineProps<{
	week: Date[];
}>();

const dayTotals = computed(() => {
	const today = new Date();

	return week.map(day => {
		const hours = timeEntriesStore.getTotalHoursForDate(day);
		return {
			dateString: formatDate(day),
			label: dayOfWeek(day.getDay()).substring(0, 2),
			hours: hours,
			hoursString: (Math.round(hours * 100) / 100).toLocaleString("nb-NO"),
			today: isSameDay(day, today),
		};
	});
});
</script>

<style scoped lang="scss">
.day-total-strip {
	display: flex;
	flex-direction: row;
	justify-content: flex-end;
	align-items: flex-end;
	gap: 1rem;
	// 1px .project-container border + 16px .project-container-content padding
	padding: 0 17px;
	margin-top: 16px;

	@media screen and (max-width: 768px) {
		gap: unset;
		justify-content: space-between;
	}
}

.day-total {
	display: flex;
	flex-direction: column;
	align-items: center;
	width: 64px;
	text-align: center;

	.day-label {
		font-size: 0.8rem;
		font-weight: 500;
		opacity: 0.7;
	}

	.day-hours {
		font-size: 1rem;
		font-weight: 600;
	}

	&.zero .day-hours {
		opacity: 0.4;
	}

	&.today {
		.day-label,
		.day-hours {
			font-weight: 700;
			opacity: 1;
		}
	}

	@media screen and (max-width: 768px) {
		width: 40px;

		.day-label {
			font-size: 0.7rem;
		}

		.day-hours {
			font-size: 0.85rem;
		}
	}
}
</style>
