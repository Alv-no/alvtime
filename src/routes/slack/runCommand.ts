import createAlvtimeClient, { Task, TimeEntrie } from "../../client/index";
import config from "../../config";
import env from "../../environment";
import UserModel, { UserData } from "../../models/user";
import configuredMoment from "../../moment";
import { capitalizeFirstLetter } from "../../utils/text";
import getAccessToken from "../auth/getAccessToken";
import sendCommandResponse from "./sendCommandResponse";
import { CommandBody } from "./slashCommand";

const { LOGG, TASKS, REG, UKE } = Object.freeze({
  TASKS: "TASKS",
  LOGG: "LOGG",
  REG: "REG",
  UKE: "UKE",
});

const client = createAlvtimeClient(env.ALVTIME_API_URI);

export default async function runCommand(commandBody: CommandBody) {
  const textArray = commandBody.text.split(" ");
  const command = textArray[0].toUpperCase();
  const params = textArray.filter((_t, i) => i !== 0);
  const userData = ((await UserModel.findById(
    commandBody.user_id
  )) as unknown) as UserData;

  switch (command) {
    case LOGG:
      logg(params, commandBody, userData);
      break;

    case TASKS:
      tasks(params, commandBody, userData);
      break;

    case REG:
      register(params, commandBody, userData);
      break;

    case UKE:
      registerWeek(params, commandBody, userData);
      break;
    default:
      break;
  }
}

async function logg(
  _params: string[],
  commandBody: CommandBody,
  userData: UserData
) {
  try {
    const accessToken = await getAccessToken(userData);
    const params = thisWeek();
    const tasksPromise = client.getTasks(accessToken);
    const timeEntriesPromise = client.getTimeEntries(params, accessToken);
    const [tasks, timeEntries] = await Promise.all([
      tasksPromise,
      timeEntriesPromise,
    ]);

    const timeEntriesWithValue = timeEntries.filter(
      (entrie: TimeEntrie) => entrie.value !== 0
    );

    const message = { text: "", response_type: "ephemeral", blocks: [] as any };
    if (timeEntriesWithValue.length === 0) {
      message.text = "Du har ikke ført noen timer denne uken :calendar:";
    } else {
      message.blocks = [
        {
          type: "section",
          text: {
            type: "mrkdwn",
            text: "Timer ført denne uken :calendar:",
          },
        },
      ];
      const week = createWeek(configuredMoment());
      for (const day of week) {
        const entries = timeEntriesWithValue.filter(
          (entrie: TimeEntrie) => entrie.date === day.format(config.DATE_FORMAT)
        );

        let text = `*${capitalizeFirstLetter(day.format("dddd D."))}*\n`;
        for (const entrie of entries) {
          const task = tasks.find((task: Task) => task.id === entrie.taskId);

          if (entrie.value !== 0) {
            text =
              text +
              capitalizeFirstLetter(`${task.name} - \`${entrie.value}\`\n`);
          }
        }
        message.blocks = [
          ...message.blocks,
          {
            type: "section",
            text: {
              type: "mrkdwn",
              text,
            },
          },
        ];
      }
    }

    sendCommandResponse(commandBody.response_url, message);
  } catch (e) {
    console.log("error", e);
  }
}

async function tasks(
  params: string[],
  commandBody: CommandBody,
  userData: UserData
) {
  try {
    const accessToken = await getAccessToken(userData);
    const tasks = await client.getTasks(accessToken);
    sendCommandResponse(commandBody.response_url, {
      text: createTasksMessage(tasks, params.includes("alle")),
    });
  } catch (e) {
    console.log("error", e);
  }
}

async function register(
  params: string[],
  commandBody: CommandBody,
  userData: UserData
) {
  try {
    const date = params[2]
      ? params[2]
      : configuredMoment().format(config.DATE_FORMAT);
    const accessToken = await getAccessToken(userData);
    const timeEntriesToEdit = [
      {
        id: 0,
        date,
        value: parseFloat(params[1].replace(",", ".")),
        taskId: parseInt(params[0]),
      },
    ];
    await client.editTimeEntries(timeEntriesToEdit, accessToken);
    logg(params, commandBody, userData);
  } catch (e) {
    console.log("error", e);
  }
}

async function registerWeek(
  params: string[],
  commandBody: CommandBody,
  userData: UserData
) {
  try {
    const accessToken = await getAccessToken(userData);
    const week = createWorkWeek(configuredMoment());
    const timeEntriesToEdit = week.map((date: string) => ({
      id: 0,
      date,
      value: parseFloat(params[1].replace(",", ".")),
      taskId: parseInt(params[0]),
    }));
    await client.editTimeEntries(timeEntriesToEdit, accessToken);
    logg(params, commandBody, userData);
  } catch (e) {
    console.log("error", e);
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

function createWeek(day: moment.Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map((n) => monday.clone().add(n, "day"));
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
