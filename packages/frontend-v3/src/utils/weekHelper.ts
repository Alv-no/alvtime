const getFirstDayOfWeek = (date: Date, weekStartsOn: number = 1): Date => {
	// weekStartsOn: 0 = Sunday, 1 = Monday (default)
	const day = date.getDay();
	const diff = (day < weekStartsOn ? 7 : 0) + day - weekStartsOn;
	const firstDay = new Date(date);
	firstDay.setDate(date.getDate() - diff);
	firstDay.setHours(0, 0, 0, 0);
	return firstDay;
};

const createWeek = (date: Date): Date[] => {
	const firstDay = getFirstDayOfWeek(date);
	const week: Date[] = [];
	for (let i = 0; i < 7; i++) {
		const day = new Date(firstDay);
		day.setDate(firstDay.getDate() + i);
		week.push(day);
	}
	return week;
};

export {
	getFirstDayOfWeek,
	createWeek,
};
