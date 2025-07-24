<template>
	<div class="input-wrapper">
		<input
			:id="`${timeEntry.taskId}-${timeEntry.date}`"
			v-model="timeValue"
			type="text"
			class="form-control"
			:class="{ 'has-value': timeEntry.value > 0 }"
			@focus="isInputActive = true"
			@blur="hideTrackButton"
			@change="updateTimeEntry(timeValue)"
		/>
		<TrackRestOfDayButton
			v-if="isInputActive"
			:currentValue="parseFloat(timeValue.replace(',', '.'))"
			:date="timeEntry.date"
			@track-rest-of-day="trackRestOfDay"
		/>
	</div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { type TimeEntry } from "@/types/TimeEntryTypes";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import TrackRestOfDayButton from "./TrackRestOfDayButton.vue";

const isInputActive = ref(false);
const timeEntriesStore = useTimeEntriesStore();

const { timeEntry } = defineProps<{
	timeEntry: TimeEntry;
}>();

const timeValue = ref<string>(timeEntry.value.toLocaleString("nb-NO"));

const updateTimeEntry = (timeValue: string) => {
	if (timeValue) {
		timeEntriesStore.updateTimeEntry({ ...timeEntry, value: parseFloat(timeValue.replace(",", ".")) });
	}
};

const trackRestOfDay = (currentValue: number) => {
	console.log("currentValue:", currentValue);
	timeValue.value = currentValue.toLocaleString("nb-NO");
	updateTimeEntry(currentValue.toLocaleString("nb-NO"));
};


const hideTrackButton = () => {
	setTimeout(() => { isInputActive.value = false; }, 200);
};

</script>

<style lang="scss" scoped>
.input-wrapper {
	position: relative;
}

input {
	border-radius: 5px;
	border: 1px solid #ccc;
	background-color: rgb(250, 245, 235);
	transition: border-color 0.3s ease;
	padding: 8px;
	width: 48px;
	text-align: center;
	font-size: 1rem;

	&.has-value {
		border-color: $primary-color;
	}
}
</style>
