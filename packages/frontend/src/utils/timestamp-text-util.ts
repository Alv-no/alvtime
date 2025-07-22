import { InvoicePeriods } from "@/store/invoiceRate";

export const mapTimeStampToLabel = (
  timeStamp: string,
  granularity: InvoicePeriods
): string => {
  switch (granularity) {
    case InvoicePeriods.Annually:
      return timeStamp.slice(0, 4);
    case InvoicePeriods.Monthly:
      return mapTimestampToMonthYearString(timeStamp);
    case InvoicePeriods.Weekly: {
      const weekNumber = getWeekNumber(timeStamp);
      return `Uke ${weekNumber}`;
    }
    case InvoicePeriods.Daily: {
      const monthString = timeStamp.slice(5, 7);
      const month = Number.parseInt(monthString, 10);
      return `${timeStamp.slice(8, 10)}. ${months[month - 1]}`;
    }
    default:
      console.warn(`Unknown granularity type: ${granularity}`);
      return "";
  }
};

/**
 * Finds the week of year that the given timestamp is in.
 * Source: https://www.geeksforgeeks.org/calculate-current-week-number-in-javascript/
 * @param {string} timestamp
 * @returns {number} - week number
 */
const getWeekNumber = (timestamp: string) => {
  const currentDate = new Date(timestamp);
  const startDate = new Date(currentDate.getFullYear(), 0, 1);
  const days = Math.floor(
    (currentDate.getTime() - startDate.getTime()) / (24 * 60 * 60 * 1000)
  );
  const weekNumber = Math.ceil(days / 7);
  return weekNumber;
};

export function mapTimestampToMonthYearString(timeStamp: string): string {
  const year = timeStamp.slice(0, 4);
  const monthString = timeStamp.slice(5, 7);
  const month = Number.parseInt(monthString, 10);

  return `${months[month - 1]} ${year}`;
}

const months = [
  "Januar",
  "Februar",
  "Mars",
  "April",
  "Mai",
  "Juni",
  "Juli",
  "August",
  "September",
  "Oktober",
  "November",
  "Desember",
];

export function getCurrentMonthName(lowerCase = false): string {
  const currentMonthNumber = new Date().getMonth();
  const month = months[currentMonthNumber];

  if (lowerCase) {
    return month.toLowerCase();
  }

  return month;
}

export function createTimeString(
  year: number,
  month: number,
  day: number
): string {
  return `${year}-${twoDigitNumber(month)}-${twoDigitNumber(day)}`;
}

function twoDigitNumber(value: number): string {
  return value < 10 ? `0${value}` : `${value}`;
}

export function decimalToRoundedPercentage(decimal: number): number {
  return Math.round(decimal * 1000) / 10;
}

export function formatDecimalArray(array: number[]): number[] {
  return array.map(item => decimalToRoundedPercentage(item));
}

export function averageFromArray(numbers: number[]): number {
  return numbers.length > 0
    ? Math.round(numbers.reduce((a, b) => a + b, 0) / numbers.length)
    : 0;
}
