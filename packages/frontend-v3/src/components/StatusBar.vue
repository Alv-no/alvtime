<template>
	<div class="status-bar-container">
		<div class="column-box">
			<div class="content-box">
				<HugeiconsIcon
					:icon="Calendar03Icon"
					class="icon"
				/> {{ totalHoursThisWeek }}/37,5
			</div>
			<div class="content-box flex">
				<div>
					<HugeiconsIcon
						:icon="MoneyBag02Icon"
						class="icon"
					/> {{ timeBankOverview?.availableHoursBeforeCompensation }}
				</div>
				<div>
					<HugeiconsIcon
						:icon="BeachIcon"
						class="icon"
					/> {{ vacation?.availableVacationDays }}
				</div>
			</div>
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
import { MoneyBag02Icon, BeachIcon, Calendar03Icon } from "@hugeicons/core-free-icons";

const vacationStore = useVacationStore();
const { vacation } = storeToRefs(vacationStore);

const timeBankStore = useTimeBankStore();
const { timeBankOverview } = storeToRefs(timeBankStore);

const { timeEntries } = storeToRefs(useTimeEntriesStore());

const { currentWeek } = storeToRefs(useDateStore());

const totalHoursThisWeek = computed(() => {
	console.log("Calculating total hours for the week", currentWeek.value);
	return timeEntries.value.reduce((total, entry) => {
		const entryDate = new Date(entry.date);
		if (entryDate >= currentWeek.value[0] && entryDate <= currentWeek.value[6]) {
			return total + entry.value;
		}
		return total;
	}, 0);
});

onMounted(async () => {
	await vacationStore.getVacationOverview();
	await timeBankStore.getTimeBankOverview();
});
</script>

<style scoped lang="scss">
	.status-bar-container {
		background-color: rgb(157, 172, 134);
		border-radius: 15px;
		width: 100%;
		display: flex;
		justify-content: flex-start;
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
			min-width: 90px;
			border-radius: 10px;
			padding: 4px 16px 12px;
			background-color: rgb(206 214 194);

			&.flex {
				display: flex;
				justify-content: space-between;
				gap: 16px;
			}
		}
	}

	.icon {
		position: relative;
		top: 5px;
	}
</style>
