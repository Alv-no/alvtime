import { createNorwegianHolidays, type NorwegianHolidays } from "@/utils/holidayHelper";
import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { createWeek, createWeeks, getRadiusOfWeeks } from "@/utils/weekHelper";
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

	const extendWeeks = async (): Promise<number> => {
		const radius = getRadiusOfWeeks();
		const newWeeks: Date[][] = [];

		const lastWeek = weeks.value[weeks.value.length - 1];
		const lastDay = lastWeek[6];

		for (let i = 1; i <= radius; i++) {
			const date = new Date(lastDay);
			date.setDate(lastDay.getDate() + i * 7);
			newWeeks.push(createWeek(date))
		}

		weeks.value = [...weeks.value, ...newWeeks];

		const years = getYearsInRange(weeks.value[0][0].getFullYear(), weeks.value[weeks.value.length - 1][6].getFullYear());
		holidays.value = createNorwegianHolidays(years);

		if (weeksDateRange.value) {
			await timeEntriesStore.getTimeEntries(weeksDateRange.value);
		}

		return newWeeks.length;
	}

	const getYearsInRange = (start: number, end: number): number[] => {
		const years: number[] = [];
		for (let y = start; y <= end; y++) {
			years.push(y);
		}
		return years;
	};

	return {
		activeDate,
		currentWeek,
		weeks,
		holidays,
		setActiveDate,
		setActiveWeekIndex,
		setSwiper,
		extendWeeks
	};
});