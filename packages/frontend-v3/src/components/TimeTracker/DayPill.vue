<template>
	<div class="day-pill">
		{{ dateString }}
	</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useDateStore } from "@/stores/dateStore";
import { dayOfWeek } from "@/utils/dateHelper";

const dateStore = useDateStore();
const { holidays } = dateStore;

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
</script>

<style scoped lang="scss">
.day-pill {
	border-radius: 5px;
	border: 1px solid #ccc;
	background-color: rgb(250, 245, 235);
	transition: border-color 0.3s ease;
	padding: 8px 8px 4px 8px;
	width: 48px;
	text-align: center;
	font-size: 1rem;
}
</style>