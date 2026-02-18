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
	default: throw new Error(`Invalid day of the week: ${day}`);
	}
};

const monthOfYear = (month: number) => {
	switch (month) {
	case 1: return "Januar";
	case 2: return "Februar";
	case 3: return "Mars";
	case 4: return "April";
	case 5: return "Mai";
	case 6: return "Juni";
	case 7: return "Juli";
	case 8: return "August";
	case 9: return "September";
	case 10: return "Oktober";
	case 11: return "November";
	case 12: return "Desember";
	default: throw new Error(`Invalid month of the year: ${month}`);
	}
};

const normalizeToMidnight = (date: Date): Date => {
	const d = new Date(date);
	d.setHours(0, 0, 0, 0);
	return d;
};

const isBefore = (date: Date, compareDate: Date): boolean => {
	return date.getTime() < compareDate.getTime();
};

const isAfter = (date: Date, compareDate: Date): boolean => {
	return !isBefore(compareDate, date);
};


const isOnOrBefore = (date: Date, compareDate: Date): boolean => {
	const d = normalizeToMidnight(date);
	const c = normalizeToMidnight(compareDate);
	return d.getTime() <= c.getTime();
};

const isOnOrAfter = (date: Date, compareDate: Date): boolean => {
	const d = normalizeToMidnight(date);
	const c = normalizeToMidnight(compareDate);
	return d.getTime() >= c.getTime();
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
	isOnOrBefore,
	isOnOrAfter,
	isBetween,
	todayIsBefore,
	todayIsAfter,
	todayIsBetween,
	isSameDay,
	formatDate,
	dayOfWeek,
	monthOfYear,
};
