<template>
	<ErrorBox
		v-if="timeEntryError?.status"
		class="time-tracker-error"
		:closable="true"
		@close="close"
	>
		{{ timeEntryError?.errors.InvalidAction[0] || "En feil oppstod ved lagring av tidsregistreringen." }}
	</ErrorBox>
</template>

<script setup lang="ts">
import ErrorBox from "@/components/utils/ErrorBox.vue";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import { storeToRefs } from "pinia";

const timeEntriesStore = useTimeEntriesStore();
const { timeEntryError } = storeToRefs(timeEntriesStore);

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
