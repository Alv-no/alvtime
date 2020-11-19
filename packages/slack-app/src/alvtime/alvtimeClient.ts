import { ReportTimeEntrie } from "../client/index";
import configuredMoment from "../moment";
import createAlvtimeClient from "../client/index";
import config from "../config";
import env from "../environment";

const alvtimeClient = createAlvtimeClient(env.ALVTIME_API_URI);

export default alvtimeClient;

export async function getLastWeekReport() {
  const sunday = configuredMoment().startOf("week").add(-1, "day");
  const monday = sunday.clone().startOf("week");
  const lastWeekDateRange = {
    fromDateInclusive: monday.format(config.DATE_FORMAT),
    toDateInclusive: sunday.format(config.DATE_FORMAT),
  };
  const timeEntriesReport = await alvtimeClient.getTimeEntriesReport(
    lastWeekDateRange,
    env.REPORT_USER_PERSONAL_ACCESS_TOKEN
  );
  return createUserReport(timeEntriesReport);
}

export interface UserReports {
  [key: string]: UserReport;
}

export interface UserReport {
  sum: number;
  entries: {
    taskId: number;
    date: string;
    value: number;
  }[];
}

function createUserReport(timeEntriesReport: ReportTimeEntrie[]): UserReports {
  const userReports: UserReports = {};
  for (const entrie of timeEntriesReport) {
    const { userEmail, taskId, date, value } = entrie;
    const email = userEmail.toLowerCase();
    if (!userReports[email]) userReports[email] = { sum: 0, entries: [] };
    userReports[email].entries.push({
      taskId: taskId,
      date: date,
      value: value,
    });
    userReports[email].sum += value;
  }
  return userReports;
}
