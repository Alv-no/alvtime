<template>
	<div class="absence-plan-summary">
		<p class="totals">
			<strong>{{ totalDaysLabel }}</strong> · {{ formatHours(plan.totalHours) }}
		</p>
		<p
			v-if="vacationBalance"
			class="vacation-balance"
		>
			Feriedager igjen: {{ formatDays(vacationBalance.before) }} → {{ formatDays(vacationBalance.after) }}
		</p>
		<p
			v-if="vacationBalance && vacationBalance.after < 0"
			class="warning"
		>
			⚠ Overstiger tilgjengelige feriedager med {{ formatDays(-vacationBalance.after) }}
		</p>
	</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { AbsencePlan } from "@/types/AbsencePlanTypes";
import { formatHours } from "@/utils/absencePlanHelper";

const HOURS_PER_VACATION_DAY = 7.5;

const { plan, availableVacationDays = undefined } = defineProps<{
	plan: AbsencePlan;
	// Kun satt når valgt kode er ferie
	availableVacationDays?: number;
}>();

const totalDaysLabel = computed(() =>
	plan.totalDays === 1 ? "1 dag" : `${plan.totalDays} dager`
);

const vacationBalance = computed(() => {
	if (availableVacationDays === undefined) return null;
	return {
		before: availableVacationDays,
		after: availableVacationDays - plan.totalHours / HOURS_PER_VACATION_DAY,
	};
});

const formatDays = (days: number) =>
	(Math.round(days * 100) / 100).toLocaleString("nb-NO");
</script>

<style scoped lang="scss">
.absence-plan-summary {
	p {
		margin: 4px 0;
	}

	.totals {
		font-size: 1.1rem;
	}

	.warning {
		color: #721c24;
		font-weight: 600;
	}
}
</style>
