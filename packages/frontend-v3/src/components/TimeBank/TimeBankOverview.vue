<template v-if="!loading">
	<h2>Overtidstimer</h2>
	<SectionedBar :sections="overtimeSections" />
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { storeToRefs } from "pinia";
import SectionedBar from "./SectionedBar.vue";
import { useTimeBankStore } from "@/stores/timeBankStore";

const loading = ref<boolean>(true);

const timeBankStore = useTimeBankStore();

const { timeBankOverview } = storeToRefs(timeBankStore);

const overtimeSections = computed(() => {
	const unspentOverTime = {
		volunteer: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 0.5).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		mandatory: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 1).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		billable: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 1.5).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		mandatoryBillable: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 2).reduce((acc, entry) => acc + entry.hours, 0) || 0,
	}
	console.log(unspentOverTime);
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
</style>