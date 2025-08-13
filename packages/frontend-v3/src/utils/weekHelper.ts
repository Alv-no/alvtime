import { createNorwegianHolidays, type NorwegianHolidays } from "@/utils/holidayHelper";
const RADIUS_OF_WEEKS = 26;
//const RADIUS_OF_DAYS = 7 * RADIUS_OF_WEEKS;

const getFirstDayOfWeek = (date: Date, weekStartsOn: number = 1): Date => {
	const d = new Date(date.toString());
	const day = d.getDay();
	// Adjust so that Monday is 0, Sunday is 6
	const diff = (day + 7 - weekStartsOn) % 7;
	d.setDate(d.getDate() - diff);
	d.setHours(0, 0, 0, 0);
	return d;
};

const getWeekNumber = (date: Date) => {
	const referenceDate = new Date(date.toString());
	if(!date) {
		date = new Date();
	}

	referenceDate.setHours(0, 0, 0, 0);
	// Thursday in current week decides the year.
	referenceDate.setDate(referenceDate.getDate() + 3 - (referenceDate.getDay() + 6) % 7);
	// January 4 is always in week 1.
	const week1 = new Date(referenceDate.getFullYear(), 0, 4);
	// Adjust to Thursday in week 1 and count number of weeks from date to week1.
	return 1 + Math.round(((referenceDate.getTime() - week1.getTime()) / 86400000
                        - 3 + (week1.getDay() + 6) % 7) / 7);
};

const createWeek = (date: Date): Date[] => {
	const firstDay = getFirstDayOfWeek(date);
	const week: Date[] = [];
	for (let i = 0; i < 7; i++) {
		const day = new Date(firstDay.getTime());
		day.setDate(day.getDate() + i);
		week.push(day);
	}
	return week;
};

const createWeeks = async (activeDate: Date) => {
	const centerDate = new Date(activeDate.toString());
	const weeks = createWeeksAround(centerDate);
	const years = numbArray(
		weeks[0][0].getFullYear(),
		weeks[weeks.length - 1][6].getFullYear()
	);
	const holidays: NorwegianHolidays = createNorwegianHolidays(years);
	
	return {
		weeks,
		holidays,
	};
};

const createWeeksAround =(centerDate: Date) => {
	const numbers = [
		...descendingNumbers(RADIUS_OF_WEEKS),
		0,
		...ascendingNumbers(RADIUS_OF_WEEKS),
	];

	const weeks = numbers
		.map(n => {
			const date = new Date(centerDate.toString());
			date.setDate(centerDate.getDate() + n * 7);
			return date;
		})
		.map(createWeek);
	return weeks;
};

const ascendingNumbers = (length: number) => {
	return createArrayOf(length, i => i + 1);
};

const descendingNumbers = (length: number) => {
	return createArrayOf(length, i => (i + 1) * -1).reverse();
};

const numbArray = (start: number, stop: number): number[] => {
	const length = stop - start + 1;
	return createArrayOf(length, (i: number) => start + i);
};

const createArrayOf = <T>(length: number, mapFunc: (index: number) => T): T[] => {
	return [...Array(length)].map((_n, i) => mapFunc(i));
};

const getInitialWeekSlide = () => {
	return RADIUS_OF_WEEKS;
};

export {
	getFirstDayOfWeek,
	getWeekNumber,
	createWeek,
	createWeeks,
	getInitialWeekSlide
};
