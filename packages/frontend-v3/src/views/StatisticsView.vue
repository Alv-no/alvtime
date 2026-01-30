<template>
	<div class="statistics-layout">
		<div class="statistics-filter-container">
			<div class="statistics-filter-item">
				<b>Fra og med måned</b>
				<VueDatePicker
					v-model="fromMonthInclusive"
					monthPicker
				/>
			</div>
			<div class="statistics-filter-item">
				<b>Til og med måned</b>
				<VueDatePicker
					v-model="toMonthInclusive"
					monthPicker
				/>
			</div>
			<div class="statistics-filter-item">
				<label for="granularity"><b>Velg oppløsning:</b></label>
				<select
					id="granularity"
					v-model="granularity"
					name="granularity"
				>
					<option value="year">
						År
					</option>
					<option
						value="month"
					>
						Måned
					</option>
					<option value="week">
						Uke
					</option>
					<option value="day">
						Dag
					</option>
				</select>
			</div>
		</div>
		<div class="content-row">
			<div class="side-content">
				<StatisticsCard
					title="Faktureringsgrad"
					:value="averageInvoiceRate +' %'"
				/>
				<StatisticsCard
					title="Fakturerbare timer"
					:value="sumBillableHours"
				/>
				<StatisticsCard
					title="Interntimer"
					:value="sumNonBillableHours"
				/>
				<StatisticsCard
					title="Fakturerbare overtidstimer"
					:value="sumBillableOvertimeHours"
				/>
				<StatisticsCard
					title="Interne overtidstimer"
					:value="sumNonBillableOvertimeHours"
				/>
			</div>
			<div class="main-content">
				<StatisticsChart
					v-if="statistics"
					:statistics="statistics"
					:period="granularityToPeriod[granularity]"
				/>
			</div>
		</div>
		<RecentHoursOverview
			v-if="timeEntryOverview && projects"
			:timeEntryOverview="timeEntryOverview"
			:projects="projects"
		/>
	</div>
</template>

<script setup lang="ts">
import { useStatisticsStore } from "@/stores/statisticsStore.ts";
import { computed, onMounted, ref, watch } from "vue";
import { storeToRefs } from "pinia";
import StatisticsCard from "@/components/Statistics/StatisticsCard.vue";
import { type TimeBankEntry, useTimeBankStore } from "@/stores/timeBankStore.ts";
import { VueDatePicker } from "@vuepic/vue-datepicker";
import "@vuepic/vue-datepicker/dist/main.css";
import StatisticsChart from "@/components/Statistics/StatisticsChart.vue";
import RecentHoursOverview from "@/components/Statistics/RecentHoursOverview.vue";
import { useTaskStore } from "@/stores/taskStore.ts";

const STORAGE_KEYS = {
	fromMonth: "statistics-fromMonthInclusive",
	toMonth: "statistics-toMonthInclusive",
	granularity: "statistics-granularity",
};

const getStoredMonth = (key: string, fallback: { month: number; year: number }) => {
	const stored = localStorage.getItem(key);
	if (stored) {
		try {
			return JSON.parse(stored);
		} catch {
			return fallback;
		}
	}
	return fallback;
};

const loading = ref<boolean>(true);

const defaultMonth = {
	month: new Date().getMonth(),
	year: new Date().getFullYear()
};

const fromMonthInclusive = ref(getStoredMonth(STORAGE_KEYS.fromMonth, defaultMonth));
const toMonthInclusive = ref(getStoredMonth(STORAGE_KEYS.toMonth, defaultMonth));
const granularity = ref<string>(localStorage.getItem(STORAGE_KEYS.granularity) ?? "month");
const granularityToPeriod: Record<string, 0 | 1 | 2 | 3> = {
	day: 0,
	week: 1,
	month: 2,
	year: 3,
};

const statisticsStore = useStatisticsStore();
const { statistics } = storeToRefs(statisticsStore);
const { timeEntryOverview } = storeToRefs(statisticsStore);

const timeBankStore = useTimeBankStore();
const { timeBankOverview } = storeToRefs(timeBankStore);

const taskStore = useTaskStore();
const { projects } = storeToRefs(taskStore);

const sumBillableHours = computed(() => {
	if (!statistics.value) return 0;
	return statistics.value.billableHours.reduce((total, period) => total + period, 0);
});

const sumNonBillableHours = computed(() => {
	if (!statistics.value) return 0;
	return statistics.value.nonBillableHours.reduce((total, period) => total + period, 0);
});

const averageInvoiceRate = computed(() => {
	if (!statistics.value || statistics.value.invoiceRate.length === 0) return 0;
	const sumInvoiceRate = statistics.value.invoiceRate.reduce((total, period) => total + period, 0);
	return ((sumInvoiceRate*100) / statistics.value.invoiceRate.length).toFixed(1);
});

const sumBillableOvertimeHours = computed(() => {
	if (!timeBankOverview.value?.entries) return 0;
	return timeBankOverview.value.entries
		.filter((entry: TimeBankEntry) => {
			const entryTime = new Date(entry.date).getTime();
			return entry.compensationRate >= 1.5
        && entry.type === 0
        && entryTime >= fromDate.value.getTime()
        && entryTime <= toDate.value.getTime();
		})
		.reduce((total: number, entry: TimeBankEntry) => total + entry.hours, 0);
});

const sumNonBillableOvertimeHours = computed(() => {
	if (!timeBankOverview.value?.entries) return 0;
	return timeBankOverview.value.entries
		.filter((entry: TimeBankEntry) => {
			const entryTime = new Date(entry.date).getTime();
			return entry.compensationRate < 1.5
        && entry.type === 0
        && entryTime >= fromDate.value.getTime()
        && entryTime <= toDate.value.getTime();
		})
		.reduce((total: number, entry: TimeBankEntry) => total + entry.hours, 0);
});

const fromDate = computed(() => {
	return new Date(fromMonthInclusive.value.year, fromMonthInclusive.value.month, 1);
});

const toDate = computed(() => {
	return new Date(toMonthInclusive.value.year, toMonthInclusive.value.month + 1, 0);
});

const fetchStatistics = async () => {
	await statisticsStore.getStatistics({
		fromDate: fromDate.value,
		toDate: toDate.value,
		period: granularityToPeriod[granularity.value],
		includeZeroPeriods: false,
	});
};

const fetchTimeEntryOverview = async () => {
	await statisticsStore.getTimeEntryOverview();
};

onMounted( async () => {
	await fetchStatistics();
	await fetchTimeEntryOverview();
	await timeBankStore.getTimeBankOverview();
	await taskStore.getTasks();
	loading.value = false;
});

watch([fromDate, toDate], async () => {
	if (loading.value) return;
	await fetchStatistics();
});

watch(granularity, async () => {
	if (loading.value) return;
	await fetchStatistics();
});

watch(fromMonthInclusive, (newVal) => {
	localStorage.setItem(STORAGE_KEYS.fromMonth, JSON.stringify(newVal));
}, { deep: true });

watch(toMonthInclusive, (newVal) => {
	localStorage.setItem(STORAGE_KEYS.toMonth, JSON.stringify(newVal));
}, { deep: true });

watch(granularity, (newVal) => {
	localStorage.setItem(STORAGE_KEYS.granularity, newVal);
});
</script>

<style scoped lang="scss">
.statistics-layout {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  height: 100%;
}

.side-content {
  flex: 0 0 20%;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.main-content {
  flex: 1;
}

.statistics-filter-container {
  display: flex;
  flex-direction: row;
  gap: 2rem;
  align-items: flex-end;
}

.content-row {
  display: flex;
  flex: 1;
  gap: 1rem;
  min-height: 0;
}

.statistics-filter-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

input {
  border-radius: 5px;
  border: 1px solid #ccc;
  background-color: white;
  transition: border-color 0.3s ease;
  padding: 8px;
  width: 48px;
  font-size: 1rem;
}

select {
  border-radius: 5px;
  border: 1px solid #ccc;
  background-color: white;
  transition: border-color 0.3s ease;
  padding: 8px;
  font-size: 1rem;
  font-family: $primary-font;
  color: $primary-color;
  cursor: pointer;

  &:focus {
    outline: none;
    border-color: $secondary-color;
  }
}
</style>