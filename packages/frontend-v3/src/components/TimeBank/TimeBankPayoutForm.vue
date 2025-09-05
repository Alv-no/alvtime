<template>
	<div class="form-container">
		<input
			id="time-bank-payout-hours"
			v-model.number="hours"
			type="number"
			placeholder="Antall timer"
			:disabled="submittingPayout"
			@focus="onInputFocus"
		/>
		<button
			:disabled="submittingPayout || (timeBankOverview?.availableHoursBeforeCompensation ?? 0) <= 0"
			@click="submitPayout"
		>
			Bestill
		</button>
	</div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { storeToRefs } from "pinia";
import { useTimeBankStore } from "@/stores/timeBankStore";

const hours = ref<number>(0);
const submittingPayout = ref(false);
const timeBankStore = useTimeBankStore();
const { timeBankOverview } = storeToRefs(timeBankStore);

const submitPayout = async () => {
	if (hours.value && timeBankOverview.value?.availableHoursBeforeCompensation && hours.value > 0 && hours.value <= timeBankOverview.value?.availableHoursBeforeCompensation) {
		submittingPayout.value = true;

		await timeBankStore.orderTimeBankPayout(hours.value);

		hours.value = 0;
		submittingPayout.value = false;
	}
};

const onInputFocus = () => {
	const inputElement = document.getElementById("time-bank-payout-hours") as HTMLInputElement;
	inputElement.select();
};

watch(hours, (newVal) => {
	if (newVal < 0) {
		hours.value = 0;
	}

	if (
		newVal &&
		timeBankOverview.value?.availableHoursBeforeCompensation &&
		newVal > timeBankOverview.value.availableHoursBeforeCompensation
	) {
		hours.value = timeBankOverview.value.availableHoursBeforeCompensation;
	}
});

</script>

<style scoped lang="scss">
.form-container {
	margin-top: 16px;
	display: flex;
	align-items: center;
	gap: 16px;

	input {
		padding: 0.5rem;
		border: 1px solid #ccc;
		border-radius: 25px;
		height: 24px;
		flex-grow: 2;
		text-align: center;
	}

	button {
		background-color: $secondary-color;
		color: $primary-color;
		border-radius: 25px;
		border: none;
		padding: 13px 24px 13px 24px;
		cursor: pointer;
		font-size: 14px;
		font-weight: 600;

		&:hover {
			background-color: $secondary-color-light;
			color: $primary-color;
		}
	}
}
</style>