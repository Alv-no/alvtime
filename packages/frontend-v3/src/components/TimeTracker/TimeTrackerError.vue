<template>
	<ErrorBox
		v-if="timeEntryError?.status"
		class="time-tracker-error"
		:closable="true"
		@close="close"
	>
		{{ getErrorMessage() }}
	</ErrorBox>
</template>

<script setup lang="ts">
import ErrorBox from "@/components/utils/ErrorBox.vue";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import { storeToRefs } from "pinia";

const timeEntriesStore = useTimeEntriesStore();
const { timeEntryError } = storeToRefs(timeEntriesStore);

const getErrorMessage = () => {
	if (!timeEntryError.value) {
		return "En feil oppstod ved lagring av tidsregistreringen.";
	}

	if (timeEntryError.value.errors) {
		const firstErrorKey = Object.keys(timeEntryError.value.errors)[0];
		if (firstErrorKey && timeEntryError.value.errors[firstErrorKey]?.[0]) {
			return timeEntryError.value.errors[firstErrorKey][0];
		}
	}

	return timeEntryError.value.title || "En feil oppstod ved lagring av tidsregistreringen.";
};

const close = () => {
	timeEntryError.value = {};
};
</script>

<style scoped lang="scss">
.time-tracker-error {
	position: fixed;
	top: 128px;
	left: 50%;
	transform: translateX(-50%);
	z-index: 10;
}
</style>
