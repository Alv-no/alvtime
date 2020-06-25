import alvtimeClient from "../alvtime/alvtimeClient";
import { Task } from "../client/index";
import config from "../config";
import { loggMessage } from "../messages/index";
import { UserData } from "../models/user";
import configuredMoment from "../moment";
import getAccessToken from "../routes/auth/getAccessToken";
import respondToResponseURL from "../response/respondToResponseURL";
import { CommandBody } from "../routes/slack/slashCommand";
import { logger } from "../createLogger"

interface State {
  accessToken: string;
  params: string[];
  commandBody: CommandBody;
  userData: UserData;
}

export default async function createCommands(
  params: string[],
  commandBody: CommandBody,
  userData: UserData
) {
  const accessToken = await getAccessToken(userData);

  const state = {
    accessToken,
    params,
    commandBody,
    userData,
  };

  return {
    logg() {
      logg(state);
    },
    tasks() {
      tasks(state);
    },
    register() {
      register(state);
    },
    registerWeek() {
      registerWeek(state);
    },
  };
}

async function logg({ commandBody, accessToken }: State) {
  try {
    const tasksPromise = alvtimeClient.getTasks(accessToken);
    const timeEntriesPromise = alvtimeClient.getTimeEntries(
      thisWeek(),
      accessToken
    );
    const [tasks, timeEntries] = await Promise.all([
      tasksPromise,
      timeEntriesPromise,
    ]);

    const message = loggMessage(timeEntries, tasks);

    respondToResponseURL(commandBody.response_url, message);
  } catch (e) {
    logger.error("error", e);
  }
}

async function tasks({ params, commandBody, accessToken }: State) {
  try {
    const tasks = await alvtimeClient.getTasks(accessToken);
    respondToResponseURL(commandBody.response_url, {
      text: createTasksMessage(tasks, params.includes("alle")),
    });
  } catch (e) {
    logger.error("error", e);
  }
}

async function register(state: State) {
  const { params, accessToken } = state;
  try {
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
  } catch (e) {
    logger.error("error", e);
  }
}

async function registerWeek(state: State) {
  try {
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
  } catch (e) {
    logger.error("error", e);
  }
}

function thisWeek() {
  const monday = configuredMoment().startOf("week");
  const sunday = monday.clone().add(7, "days");

  return {
    fromDateInclusive: monday.format(config.DATE_FORMAT),
    toDateInclusive: sunday.format(config.DATE_FORMAT),
  };
}

function createWorkWeek(day: moment.Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4].map((n) =>
    monday.clone().add(n, "day").format(config.DATE_FORMAT)
  );
}

function createTasksMessage(tasks: Task[], all: boolean) {
  let text = "*ID* - *Task* - *Prosjekt* - *Kunde*\n";
  for (const task of tasks) {
    if (task.favorite || all)
      text =
        text +
        `${task.id} - ${task.name} - ${task.project.name} - ${task.project.customer.name}\n`;
  }
  return text;
}
