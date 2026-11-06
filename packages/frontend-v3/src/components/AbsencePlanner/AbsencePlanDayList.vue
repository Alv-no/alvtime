<template>
	<div class="absence-plan-day-list">
		<button
			type="button"
			class="toggle"
			:aria-expanded="expanded"
			@click="expanded = !expanded"
		>
			<FeatherIcon
				:name="expanded ? 'chevron-down' : 'chevron-right'"
				:size="16"
			/>
			{{ expanded ? "Skjul" : "Vis" }} alle dager
		</button>
		<div v-if="expanded">
			<section
				v-for="week in weeks"
				:key="week.key"
				class="week"
			>
				<h4>{{ week.label }}</h4>
				<ul>
					<li
						v-for="day in week.days"
						:key="day.date"
						:class="{ inactive: day.status !== 'Register' }"
					>
						<span class="day-label">{{ formatDayLabel(day.date) }}</span>
						<span>{{ describeDay(day, holidayNames.get(day.date)) }}</span>
					</li>
				</ul>
			</section>
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import FeatherIcon from "@/components/utils/FeatherIcon.vue";
import type { AbsencePlanDay } from "@/types/AbsencePlanTypes";
import { createNorwegianHolidays } from "@/utils/holidayHelper";
import { describeDay, formatDayLabel, groupByWeek, parseDate } from "@/utils/absencePlanHelper";

const { days } = defineProps<{
	days: AbsencePlanDay[];
}>();

const expanded = ref(false);

const weeks = computed(() => groupByWeek(days));

// Navn på røde dager hentes fra frontend; backend avgjør hvilke dager som er røde
const holidayNames = computed(() => {
	const redDays = days.filter(d => d.status === "RedDay");
	const years = [...new Set(redDays.map(d => parseDate(d.date).getFullYear()))];
	const holidays = createNorwegianHolidays(years);
	return new Map(redDays.map(d => [d.date, holidays.getHoliday(parseDate(d.date))?.description]));
});
</script>

<style scoped lang="scss">
.absence-plan-day-list {
	margin-top: 12px;

	.toggle {
		display: flex;
		align-items: center;
		gap: 4px;
		background: none;
		border: none;
		padding: 0;
		color: $primary-color;
		font-weight: 600;
		cursor: pointer;
	}

	.week h4 {
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

		&.inactive {
			opacity: 0.55;
		}
	}

	.day-label {
		min-width: 110px;
		font-variant-numeric: tabular-nums;
	}
}
</style>
