<template>
	<div class="status-bar-container">
		<div class="column-box">
			<div class="content-box super-index">
				<WeekSummary
					:totalHoursThisWeek="totalHoursThisWeek"
					:totalHoursEachDayThisWeek="totalHoursEachDayThisWeek"
				/>
			</div>
			<div class="content-box flex">
				<div class="icon-wrapper">
					<HugeiconsIcon
						:icon="MoneyBag02Icon"
						class="icon"
					/> {{ timeBankOverview?.availableHoursBeforeCompensation }}
				</div>
				<div class="icon-wrapper">
					<HugeiconsIcon
						:icon="BeachIcon"
						class="icon"
					/> {{ vacation?.availableVacationDays }}
				</div>
			</div>
		</div>
		<div class="column-box">
			<InvoiceRateVisualizer />
		</div>
	</div>
</template>

<script setup lang="ts">
import { onMounted, computed } from "vue";
import { useVacationStore } from "@/stores/vacationStore";
import { useTimeBankStore } from "@/stores/timeBankStore";
import { useDateStore } from "@/stores/dateStore";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import { storeToRefs } from "pinia";
import { HugeiconsIcon } from "@hugeicons/vue";
import { MoneyBag02Icon, BeachIcon } from "@hugeicons/core-free-icons";
import InvoiceRateVisualizer from "./utils/InvoiceRateVisualizer.vue";
import WeekSummary from "./StatusBarComponents/WeekSummary.vue";
import { isOnOrBefore, isOnOrAfter } from "@/utils/dateHelper";

const vacationStore = useVacationStore();
const { vacation } = storeToRefs(vacationStore);

const timeBankStore = useTimeBankStore();
const { timeBankOverview } = storeToRefs(timeBankStore);

const { timeEntries } = storeToRefs(useTimeEntriesStore());

const { currentWeek } = storeToRefs(useDateStore());

const totalHoursThisWeek = computed(() => {
	return timeEntries.value.reduce((total, entry) => {
		const entryDate = new Date(entry.date);

		if (isOnOrAfter(entryDate, currentWeek.value[0]) && isOnOrBefore(entryDate, currentWeek.value[6])) {
			return total + entry.value;
		}
		return total;
	}, 0);
});

const totalHoursEachDayThisWeek = computed(() => {
	return currentWeek.value.map((date) => {
		const totalForDay = timeEntries.value.reduce((total, entry) => {
			const entryDate = new Date(entry.date);
			if (entryDate.toDateString() === date.toDateString()) {
				return total + entry.value;
			}
			return total;
		}, 0);
		return {
			date: date,
			hours: totalForDay
		};
	}).sort((a, b) => a.date.getTime() - b.date.getTime());
});

onMounted(async () => {
	vacationStore.getVacationOverview();
	timeBankStore.getTimeBankOverview();
});
</script>

<style scoped lang="scss">
	.status-bar-container {
		background-color: rgb(157, 172, 134);
		border-radius: 15px;
		width: 100%;
		display: flex;
		justify-content: space-between;
		align-items: flex-start;

		.column-box {
			display: flex;
			flex-direction: column;
			justify-content: center;
			align-items: flex-start;
			padding: 16px;
			gap: 8px;
		}

		.content-box {
			display: flex;
			align-items: center;
			justify-content: center;
			border-radius: 10px;
			padding: 12px 12px 8px;
			background-color: rgb(206 214 194);

			&.flex {
				display: flex;
				justify-content: space-between;
				gap: 16px;
			}

			&.super-index {
				z-index: 10;
			}
		}
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
