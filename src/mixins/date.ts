import moment from "moment";

export function createWeek(day: moment.Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map(n => monday.clone().add(n, "day"));
}

