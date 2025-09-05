<template v-if="!loading">
	<h2>Overtidstimer</h2>
	<div v-if="noOvertime">
		<p>Du har ingen overtidstimer i timebanken.</p>
	</div>
	<template v-else>
		<SectionedBar :sections="overtimeSections" />
		<TimeBankPayoutForm />
	</template>
	<TimeBankHistory />
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { storeToRefs } from "pinia";
import SectionedBar from "./SectionedBar.vue";
import { useTimeBankStore } from "@/stores/timeBankStore";
import TimeBankPayoutForm from "./TimeBankPayoutForm.vue";
import TimeBankHistory from "./TimeBankHistory.vue";

const loading = ref<boolean>(true);

const timeBankStore = useTimeBankStore();
const { timeBankOverview } = storeToRefs(timeBankStore);

const overtimeSections = computed(() => {
	const unspentOverTime = {
		volunteer: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 0.5).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		mandatory: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 1).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		billable: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 1.5).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		mandatoryBillable: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 2).reduce((acc, entry) => acc + entry.hours, 0) || 0,
	};

	return [
		{
			title: "Frivillig",
			amount: unspentOverTime.volunteer,
			color: "yellow",
		},
		{
			title: "Pålagt",
			amount: unspentOverTime.mandatory,
			color: "blue",
		},
		{
			title: "Fakturerbart",
			amount: unspentOverTime.billable,
			color: "green",
		},
		{
			title: "Tommy Time",
			amount: unspentOverTime.mandatoryBillable,
			color: "red",
		}
	];
});

const noOvertime = computed(() => {
	return overtimeSections.value.every(section => section.amount === 0);
});

onMounted(async () => {
	await timeBankStore.getTimeBankOverview();
	loading.value = false;
});

</script>

<style scoped lang="scss">
h2 {
	margin-top: 32px;
	margin-bottom: 8px;
}

p {
	margin: 2rem;
	text-align: center;
}
</style>