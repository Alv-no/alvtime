import alvtimeClient from "../alvtime/alvtimeClient";
import config from "../config";
import { loggMessage } from "../messages/index";
import configuredMoment from "../moment";
import respondToResponseURL from "../response/respondToResponseURL";
import { State } from "./index";
export async function logg({ commandBody, accessToken }: State) {
  const tasksPromise = alvtimeClient.getTasks(accessToken);
  const period = thisWeek();
  const timeEntriesPromise = alvtimeClient.getTimeEntries(period, accessToken);
  const [tasks, timeEntries] = await Promise.all([
    tasksPromise,
    timeEntriesPromise,
  ]);
  const message = loggMessage(timeEntries, tasks);
  respondToResponseURL(commandBody.response_url, message);
}
function thisWeek() {
  const monday = configuredMoment().startOf("week");
  const sunday = monday.clone().add(6, "days");
  return {
    fromDateInclusive: monday.format(config.DATE_FORMAT),
    toDateInclusive: sunday.format(config.DATE_FORMAT),
  };
}
