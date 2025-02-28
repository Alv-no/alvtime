import { InvoicePeriods } from "@/store/invoiceRate";

export const mapTimeStampToLabel = (
  timeStamp: string,
  granularity: InvoicePeriods
): string => {
  switch (granularity) {
    case InvoicePeriods.Annualy:
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
 * @param {string} timestamp
 * @returns {number} - week number
 */
const getWeekNumber = (timestamp: string) => {
  const currentDate = new Date(timestamp);
  //Set time to midday to avoid timezone issues
  currentDate.setUTCHours(currentDate.getUTCHours() + 12);
  currentDate.setUTCDate(currentDate.getUTCDate() + 4 - (currentDate.getUTCDay() || 7));
  const yearStart = new Date(Date.UTC(currentDate.getUTCFullYear(), 0, 1));
  const weekNumber = Math.ceil(((currentDate.getTime() - yearStart.getTime()) / (7 * 24 * 60 * 60 * 1000)));
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
