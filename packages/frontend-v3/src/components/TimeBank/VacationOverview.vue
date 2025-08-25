<template>
	<div
		v-if="!loading"
	>
		<h2>Betalte feriedager</h2>
		<SectionedBar :sections="vacationSections" />
	</div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { storeToRefs } from "pinia";
import { useVacationStore } from "@/stores/vacationStore";
import SectionedBar from "./SectionedBar.vue";

const loading = ref<boolean>(true);

const vacationStore = useVacationStore();
const { vacation } = storeToRefs(vacationStore);

const vacationSections = computed(() => {
	return [
		{
			title: "Brukt",
			amount: vacation.value?.usedVacationDaysThisYear || 0,
			color: "yellow",
		},
		{
			title: "Tilgjengelig",
			amount: vacation.value?.availableVacationDays || 0,
			color: "green",
		},
		{
			title: "Planlagt",
			amount: vacation.value?.plannedVacationDaysThisYear || 0,
			color: "blue",
		}
	];
});

onMounted( async () => {
	await vacationStore.getVacationOverview();
	loading.value = false;
});
</script>

<style scoped lang="scss">
h2 {
	margin-top: 32px;
	margin-bottom: 8px;
}
</style>