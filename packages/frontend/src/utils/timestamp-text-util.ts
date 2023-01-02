export function mapTimestampToMothYearString(timeStamp: string): string {
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
  return Math.round(numbers.reduce((a, b) => a + b, 0) / numbers.length);
}
