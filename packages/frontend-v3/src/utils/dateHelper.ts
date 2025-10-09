/**
 * Utility functions for date comparisons and formatting.
 */

const dayOfWeek = (day: number) => {
	switch (day) {
	case 0: return "Søndag";
	case 1: return "Mandag";
	case 2: return "Tirsdag";
	case 3: return "Onsdag";
	case 4: return "Torsdag";
	case 5: return "Fredag";
	case 6: return "Lørdag";
	default: throw new Error("Invalid day of the week");
	}
};

const isBefore = (date: Date, compareDate: Date): boolean => {
	return date.getTime() < compareDate.getTime();
};

const isAfter = (date: Date, compareDate: Date): boolean => {
	return !isBefore(compareDate, date);
};

const todayIsBefore = (compareDate: Date): boolean => {
	const today = new Date();
	return isBefore(today, compareDate);
};

const todayIsAfter = (compareDate: Date): boolean => {
	const today = new Date();
	return isAfter(today, compareDate);
};

const isBetween = (date: Date, startDate: Date, endDate: Date): boolean => {
	return isAfter(date, startDate) && isBefore(date, endDate);
};

const todayIsBetween = (startDate: Date, endDate: Date): boolean => {
	return isBetween(new Date(), startDate, endDate);
};

const isSameDay = (date1: Date, date2: Date): boolean => {
	return date1.getFullYear() === date2.getFullYear() &&
		date1.getMonth() === date2.getMonth() &&
		date1.getDate() === date2.getDate();
};

const adjustTimeOfDay = (date: Date): Date => {
	return new Date(date.getTime() + 2 * 60 * 60 * 1000);
};

const formatDate = (date: Date): string => {
	const year = date.getFullYear();
	let month = String(date.getMonth() + 1).padStart(2, "0");
	let day = String(date.getDate()).padStart(2, "0");

	if(month.length < 2) month = `0${month}`;
	if(day.length < 2) day = `0${day}`;

	return `${year}-${month}-${day}`;
};

export {
	isBefore,
	isAfter,
	isBetween,
	todayIsBefore,
	todayIsAfter,
	todayIsBetween,
	isSameDay,
	formatDate,
	dayOfWeek,
	adjustTimeOfDay,
};
