<template>
	<div>
		<input
			:id="`${timeEntry.taskId}-${timeEntry.date}`"
			v-model="timeValue"
			type="text"
			class="form-control"
		/>
	</div>
</template>

<script lang="ts" setup>
import { computed } from "vue";
import { type TimeEntry } from "@/types/TimeEntryTypes";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";

const timeEntriesStore = useTimeEntriesStore();

const { timeEntry } = defineProps<{
	timeEntry: TimeEntry;
}>();

const updateTimeEntry = (timeEntry: TimeEntry) => {
	if (timeEntry.value) {
		timeEntriesStore.updateTimeEntry(timeEntry);
	}
};

const timeValue = computed({
	get: () => timeEntry.value.toLocaleString('nb-NO'),
	set: (newValue: string) => {
		console.log("Setting new time value:", newValue);
		const newNumber = parseFloat(newValue.replace(",", "."));
		updateTimeEntry({ ...timeEntry, value: newNumber });
	}
});
</script>

<style lang="scss" scoped>
input {
	border-radius: 5px;
	border: 1px solid #ccc;
	background-color: rgb(250, 245, 235);
	transition: border-color 0.3s ease;
	padding: 8px;
	width: 32px;
	text-align: center;
	font-size: 1rem;
}
</style>
