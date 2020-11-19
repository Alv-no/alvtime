import alvtimeClient from "../alvtime/alvtimeClient";
import config from "../config";
import configuredMoment from "../moment";
import { logg } from "./logg";
import { State } from "./index";
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
  return [0, 1, 2, 3, 4].map((n) => monday.clone().add(n, "day").format(config.DATE_FORMAT));
}

