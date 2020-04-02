import moment from "moment";
import config from "@/config";
import { FrontendTimentrie } from "@/store/timeEntries";

export function createWeek(day: moment.Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map(n => monday.clone().add(n, "day"));
}

export function weekTimeEntrieSum(
  activeDate: moment.Moment,
  timeEntries: FrontendTimentrie[]
): string {
  const week = createWeek(activeDate).map((date: moment.Moment) =>
    date.format(config.DATE_FORMAT)
  );
  const number = timeEntries.reduce((acc: number, curr: FrontendTimentrie) => {
    if (week.indexOf(curr.date) !== -1 && !isNaN(Number(curr.value))) {
      return (acc = acc + Number(curr.value));
    } else {
      return acc;
    }
  }, 0);
  const str = number.toString().replace(".", ",");
  return str;
}
