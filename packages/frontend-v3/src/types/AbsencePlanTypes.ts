export type AbsencePlanTask = {
	id: number;
	name: string;
	isPaidHoliday: boolean;
};

export type AbsencePlanDayStatus =
	| "Register"
	| "Weekend"
	| "RedDay"
	| "AlreadyRegistered"
	| "HasOtherHours"
	| "Blocked";

export type AbsencePlanDay = {
	date: string;
	status: AbsencePlanDayStatus;
	hours: number;
	existingOtherHours: number;
	reason?: string;
};

export type AbsencePlan = {
	days: AbsencePlanDay[];
	totalDays: number;
	totalHours: number;
};

export type AbsencePlanParams = {
	taskId: number;
	fromDateInclusive: Date;
	toDateInclusive: Date;
	percentage: number;
	fillPartialDays: boolean;
	comment?: string;
};
