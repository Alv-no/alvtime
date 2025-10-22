<template>
	<button
		v-if="timeLeftInWorkday > 0"
		:class="{ 'first-day-of-week': isFirstDayOfWeek }"
		@click="logRestOfDay"
	>
		{{ timeLeftInWorkday.toLocaleString("nb-NO") }}
	</button>
</template>

<script lang="ts" setup>
import { onMounted, ref, computed } from "vue";
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

const isFirstDayOfWeek = computed(() => {
	const day = new Date(date).getDay();
	return day === 1; // Monday is the first day of the week
});

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

	@media screen and (max-width: 768px) {
		left: -42px;
		top: 2px;
		padding: 5px 10px;
		font-size: 0.75rem;

		&.first-day-of-week {
			left: unset;
			right: -42px;
		}
	}
}
</style>
