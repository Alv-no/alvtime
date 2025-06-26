import easterCalculator from "date-easter";
import { isSameDay } from "./dateHelper";

export interface Holiday {
  date: Date;
  description: string;
};

export interface NorwegianHolidays {
  isHoliday: (date: Date) => boolean;
  getHoliday: (date: Date) => Holiday | undefined;
};

export {
	createNorwegianHolidays,
};

const createNorwegianHolidays = (years: number[]) => {
  	const holidays = years.reduce((acc: Holiday[], year: number) => {
    	const holidays = createNorwegianHolidaysForYear(year);
    	return [...acc, ...holidays];
  	}, []);

  	const state = {
    	holidays,
  	};

  	return {
    	...createIsHoliday(state),
    	...createGetHoliday(state),
  	};
};

const createNorwegianHolidaysForYear = (yearNumber: number) => {
  	const yearString = yearNumber.toString();
  	const { year, month, day } = easterCalculator.gregorianEaster(yearNumber);
  	const easter = new Date(`${year}-${month}-${day}`);

	const easterDateOffset = (daysToOffset: number, easter: Date) => {
		return new Date(easter.getFullYear(), easter.getMonth(), easter.getDate() + daysToOffset);
	};

  	const norwegianHolidays = [
    	{ date: new Date(yearString + "-01-01"), description: "Første nyttårsdag" },
		{ date: easterDateOffset(-7, easter), description: "Palme-søndag" },
		{ date: easterDateOffset(-3, easter), description: "Skjær-torsdag" },
		{ date: easterDateOffset(-2, easter), description: "Lang-fredag" },
		{ date: easter, description: "Første påskedag" },
		{ date: easterDateOffset(1, easter), description: "Andre påskedag" },
		{
			date: new Date(yearString + "-05-01"),
			description: "Arbeidernes dag",
		},
		{ date: new Date(yearString + "-05-17"), description: "Grunnlovs-dag" },
		{
			date: easterDateOffset(39, easter),
			description: "Kristi himmelfart",
		},
		{ date: easterDateOffset(49, easter), description: "Første pinsedag" },
		{ date: easterDateOffset(50, easter), description: "Andre pinsedag" },
		{ date: new Date(yearString + "-12-24"), description: "Juleaften" },
		{ date: new Date(yearString + "-12-25"), description: "Første juledag" },
		{ date: new Date(yearString + "-12-26"), description: "Andre juledag" },
		{ date: new Date(yearString + "-12-31"), description: "Nyttårs-aften" },
  	];

  	return norwegianHolidays;
};

function createIsHoliday(state: { holidays: Holiday[] }) {
  	function isHoliday(date: Date) {
    	return state.holidays.some((holiday: Holiday) =>
			isSameDay(holiday.date, date)
    	);
  	}

  	return { isHoliday };
};

function createGetHoliday(state: { holidays: Holiday[] }) {
  	function getHoliday(date: Date) {
    	return state.holidays.find((holiday: Holiday) =>
      		isSameDay(holiday.date, date)
    	);
  	}

  	return { getHoliday };
};