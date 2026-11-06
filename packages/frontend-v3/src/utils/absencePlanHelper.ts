import type { AbsencePlanDay } from "@/types/AbsencePlanTypes";
import { getWeekNumber } from "./weekHelper";

const SHORT_DAY_NAMES = ["sø", "ma", "ti", "on", "to", "fr", "lø"];

// "2026-07-30" tolkes som lokal dato (new Date("2026-07-30") gir UTC)
const parseDate = (date: string): Date => {
	const [year, month, day] = date.split("-").map(Number);
	return new Date(year, month - 1, day);
};

const formatDayLabel = (date: string): string => {
	const d = parseDate(date);
	const day = String(d.getDate()).padStart(2, "0");
	const month = String(d.getMonth() + 1).padStart(2, "0");
	return `${SHORT_DAY_NAMES[d.getDay()]} ${day}.${month}.${d.getFullYear()}`;
};

const formatHours = (hours: number): string =>
	`${hours.toLocaleString("nb-NO")}t`;

const describeDay = (day: AbsencePlanDay, holidayName?: string): string => {
	switch (day.status) {
	case "Register": return formatHours(day.hours);
	case "Weekend": return "Helg";
	case "RedDay": return holidayName ?? "Rød dag";
	case "AlreadyRegistered": return "Allerede registrert";
	case "HasOtherHours": return `Har ${formatHours(day.existingOtherHours)} på andre timekoder`;
	case "Blocked": return day.reason ?? "Kan ikke registreres";
	}
};

type WeekGroup = { key: string; label: string; days: AbsencePlanDay[] };

const groupByWeek = (days: AbsencePlanDay[]): WeekGroup[] => {
	const groups: WeekGroup[] = [];
	for (const day of days) {
		const date = parseDate(day.date);
		const week = getWeekNumber(date);
		// Uke 1 kan starte i desember, uke 52/53 kan slutte i januar
		const isoYear = date.getMonth() === 11 && week === 1
			? date.getFullYear() + 1
			: date.getMonth() === 0 && week >= 52 ? date.getFullYear() - 1 : date.getFullYear();
		const key = `${isoYear}-${week}`;
		const last = groups[groups.length - 1];
		if (last?.key === key) {
			last.days.push(day);
		} else {
			groups.push({ key, label: `Uke ${week}`, days: [day] });
		}
	}
	return groups;
};

export { parseDate, formatDayLabel, formatHours, describeDay, groupByWeek, type WeekGroup };
