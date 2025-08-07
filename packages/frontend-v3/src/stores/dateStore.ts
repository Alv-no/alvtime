import type { NorwegianHolidays } from "@/utils/holidayHelper";
import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { createWeeks } from "@/utils/weekHelper";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import Swiper from "swiper";

const timeEntriesStore = useTimeEntriesStore();

// This store manages the current active date and the weeks around it, including holidays.
export const useDateStore = defineStore("date", () => {
	const activeDate = ref<Date>(new Date());
	const weeks = ref<Date[][]>([]);
	const holidays = ref<NorwegianHolidays>();
	const swiper = ref<Swiper | null>(null);
	const activeWeekIndex = ref<number>(0);

	// Function to set the active date
	const setActiveDate = async (date: Date) => {
		activeDate.value = date || new Date();
		await createWeeksInStore(activeDate.value);
		if (weeksDateRange.value) {
			await timeEntriesStore.getTimeEntries(weeksDateRange.value);
		}
	};

	const setActiveWeekIndex = (index: number) => {
		activeWeekIndex.value = index;
	};

	const createWeeksInStore = async (activeDate: Date) => {
		const createdWeeks = await createWeeks(activeDate);

		weeks.value = createdWeeks.weeks;
		holidays.value = createdWeeks.holidays;
	};

	const weeksDateRange = computed(() => {
		if (!weeks.value[0][0]) return;

		const firstDateInWeeks = weeks.value[0][0];
		const lastDateInWeeks = weeks.value[weeks.value.length - 1][6];
		const fromDateInclusive = firstDateInWeeks;
		const toDateInclusive = lastDateInWeeks;
		return { fromDateInclusive, toDateInclusive };
	});

	const setSwiper = (swiperInstance: Swiper) => {
		swiper.value = swiperInstance;
	};

	const currentWeek = computed(() => {
		return weeks.value[activeWeekIndex.value] || [];
	});

	return {
		activeDate,
		currentWeek,
		weeks,
		holidays,
		setActiveDate,
		setActiveWeekIndex,
		setSwiper,
	};
});