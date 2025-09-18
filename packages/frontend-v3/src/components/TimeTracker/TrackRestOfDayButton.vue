<template>
	<button
		v-if="timeLeftInWorkday > 0"
		@click="logRestOfDay"
	>
		{{ timeLeftInWorkday.toLocaleString("nb-NO") }}
	</button>
</template>

<script lang="ts" setup>
import { defineProps, defineEmits, onMounted, ref } from "vue";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";

const timeLeftInWorkday = ref<number>(0);
const timeEntriesStore = useTimeEntriesStore();

const { getRemainingTimeInWorkday } = timeEntriesStore;

const { currentValue, date } = defineProps<{
	currentValue: number;
	date: string;
}>();

const emit = defineEmits(["track-rest-of-day"]);

const logRestOfDay = () => {
	emit("track-rest-of-day", timeLeftInWorkday.value + currentValue);
};

onMounted(() => {
	// Calculate the remaining time in the workday when the component is mounted.
	timeLeftInWorkday.value = getRemainingTimeInWorkday(date);
});
</script>

<style lang="scss" scoped>
button {
	background-color: $background-color;
	border: 1px solid $primary-color;
	border-radius: 25px;
	padding: 8px 16px;
	font-size: 1rem;
	z-index: 5;
	position: absolute;
	left: -60px;
}
</style>