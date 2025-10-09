<template>
	<div
		class="day-pill"
		:class="{ holiday, weekend, 'is-complete': noTimeRemainingInWorkday, today }"
		@mouseover="!isMobile ? setIsHovering(true) : null"
		@mouseleave="setIsHovering(false)"
	>
		<span v-if="!isMobile">{{ !holiday || isHovering ? dateString : holiday.description }}</span>
		<span v-else>{{ dateString }}</span>
	</div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { useDateStore } from "@/stores/dateStore";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import { dayOfWeek } from "@/utils/dateHelper";

const isHovering = ref(false);

const setIsHovering = (value: boolean) => {
	isHovering.value = value;
};

const dateStore = useDateStore();
const { holidays } = dateStore;

const timeEntriesStore = useTimeEntriesStore();

const { day } = defineProps<{
	day: Date;
}>();

const isMobile = window.innerWidth <= 768;

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
	padding: 8px 2px 4px 2px;
	width: 64px;
	height: 32px;
	text-align: center;
	vertical-align: baseline;
	font-size: 1rem;
	font-weight: 500;

	&.weekend {
		background-color: #f0f0f0;
	}

	&.holiday {
		background-color: #f8d7da;
		border-color: #f5c6cb;
		color: #721c24;
		font-size: 0.8rem;

		@media screen and (max-width: 768px) {
			width: 40px;
			height: auto;
			font-size: 0.75rem;
			font-weight: 600;
		}

		@media screen and (min-width: 769px) {
			&:hover {
				font-size: 1rem;
			}
		}
	}

	&.today {
		outline: 3px solid $primary-color;
		font-weight: 700;

		@media screen and (max-width: 768px) {
			outline: 2px solid $primary-color;
		}
	}

	&.is-complete {
		outline: 2px solid $secondary-color;

		&.today {
			outline: 3px solid $secondary-color;
			font-weight: 700;
		}

		@media screen and (max-width: 768px) {
			outline: 1px solid $secondary-color;

			&.today {
				outline: 2px solid $secondary-color;
			}
		}
	}

	@media screen and (max-width: 768px) {
		width: 40px;
		height: auto;
		font-size: 0.70rem;
		font-weight: 600;
		padding: 8px 1px 6px 1px;
	}
}
</style>