import alvtimeClient from "../alvtime/alvtimeClient";
import config from "../config";
import configuredMoment from "../moment";
import { State } from "./index";
import { logg } from "./logg";
import { vacationLoggMessage } from "../messages";
import respondToResponseURL from "../response/respondToResponseURL";

export async function register(state: State) {
  const { params, accessToken } = state;
  const date = params[2]
    ? params[2]
    : configuredMoment().format(config.DATE_FORMAT);
  const timeEntriesToEdit = [
    {
      id: 0,
      date,
      value: parseFloat(params[1].replace(",", ".")),
      taskId: parseInt(params[0]),
    },
  ];
  await alvtimeClient.editTimeEntries(timeEntriesToEdit, accessToken);
  logg(state);
}

export async function registerWeek(state: State) {
  const { accessToken, params } = state;
  const week = createWorkWeek(configuredMoment());
  const timeEntriesToEdit = week.map((date: string) => ({
    id: 0,
    date,
    value: parseFloat(params[1].replace(",", ".")),
    taskId: parseInt(params[0]),
  }));
  await alvtimeClient.editTimeEntries(timeEntriesToEdit, accessToken);
  logg(state);
}
function createWorkWeek(day: moment.Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4].map((n) =>
    monday.clone().add(n, "day").format(config.DATE_FORMAT)
  );
}

export async function registerVacation(state: State) {
  const { accessToken, params, commandBody } = state;
  // hente fridager
  const fromDateInclusive = params[0];
  const toDateInclusive = params[1];
  if (
    !configuredMoment(fromDateInclusive).isValid() ||
    !configuredMoment(toDateInclusive).isValid()
  )
    return;
  const fromDate = configuredMoment(fromDateInclusive);
  const toDate = configuredMoment(toDateInclusive);
  if (fromDate.isAfter(toDate)) return;
  const fromYearInclusive = fromDate.year();
  const toYearInclusive = toDate.year();
  const apiHolidays = await alvtimeClient.getHolidays({
    fromYearInclusive,
    toYearInclusive,
  });
  const holidays = apiHolidays.map((date: string) => configuredMoment(date));

  // lage array med dager fra dato til dato
  const timeEntriesToEdit = getRange(fromDate, toDate, "days")
    // filtrere vekk fridager og helger
    .filter((date: moment.Moment) => {
      if (date.weekday() === 5 || date.weekday() === 6) return false;
      return !holidays.some((holiday: moment.Moment) => holiday.isSame(date));
    })
    // map til timeEntriesToEdit
    .map((date: moment.Moment) => ({
      id: 0,
      date: date.format(config.DATE_FORMAT),
      value: 7.5,
      taskId: 13,
    }));

  await alvtimeClient.editTimeEntries(timeEntriesToEdit, accessToken);
  const message = vacationLoggMessage({ fromDateInclusive, toDateInclusive });
  respondToResponseURL(commandBody.response_url, message);
}

function getRange(
  fromDate: moment.Moment,
  toDate: moment.Moment,
  type: moment.unitOfTime.Diff
) {
  const diff = toDate.diff(fromDate, type) + 1;
  const range = [];
  for (let i = 0; i < diff; i++) {
    range.push(configuredMoment(fromDate).add(i, type));
  }
  return range;
}
