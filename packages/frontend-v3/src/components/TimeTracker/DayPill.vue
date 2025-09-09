<template>
	<div
		class="day-pill"
		:class="{ holiday, weekend, 'is-complete': noTimeRemainingInWorkday, today }"
		@mouseover="isHovering = true"
		@mouseleave="isHovering = false"
	>
		<span>{{ !holiday || isHovering ? dateString : holiday.description }}</span>
	</div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useDateStore } from "@/stores/dateStore";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import { dayOfWeek } from "@/utils/dateHelper";

const isHovering = ref(false);

const dateStore = useDateStore();
const { holidays } = dateStore;

const timeEntriesStore = useTimeEntriesStore();

const { day } = defineProps<{
	day: Date;
}>();

const dateString = computed(() => {
	return `${dayOfWeek(day.getDay()).substring(0, 2)}. ${day.getDate()}`;
});

const holiday = computed(() => {
	if (holidays?.isHoliday(day)) {
		return holidays.getHoliday(day);
	} else {
		return null;
	}
});

const weekend = computed(() => {
	return day.getDay() === 0 || day.getDay() === 6;
});

const noTimeRemainingInWorkday = computed(() => {
	const timeZonedDate = new Date(day.getTime() - day.getTimezoneOffset() * 60000);
	const dayStr = timeZonedDate.toISOString().split("T")[0];
	return timeEntriesStore.getRemainingTimeInWorkday(dayStr) <= 0;
});

const today = computed(() => {
	const today = new Date();
	return (
		day.getDate() === today.getDate() &&
		day.getMonth() === today.getMonth() &&
		day.getFullYear() === today.getFullYear()
	);
});
</script>

<style scoped lang="scss">
.day-pill {
	display: flex;
	align-items: center;
	justify-content: center;
	border-radius: 5px;
	border: 1px solid #ccc;
	background-color: rgb(250, 245, 235);
	transition: border-color 0.3s ease;
	padding: 8px 8px 4px 8px;
	width: 48px;
	height: 24px;
	text-align: center;
	vertical-align: baseline;
	font-size: 1rem;

	&.weekend {
		background-color: #f0f0f0;
	}

	&.holiday {
		background-color: #f8d7da;
		border-color: #f5c6cb;
		color: #721c24;

		&:hover {
			font-size: 1rem;
		}
	}

	&.today {
		outline: 3px solid $primary-color;
		font-weight: 700;
	}

	&.is-complete {
		outline: 2px solid $secondary-color;

		&.today {
			outline: 3px solid $secondary-color;
			font-weight: 700;
		}
	}
}
</style>