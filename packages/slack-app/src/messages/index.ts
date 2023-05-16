import jwt from "jwt-simple";
import { Moment } from "moment";
import { Task, TimeEntrie, DateRange } from "../client/index";
import config from "../config";
import env from "../environment";
import configuredMoment from "../moment";
import { UserReport } from "../alvtime/alvtimeClient";

export interface TokenPayload {
  slackUserName: string;
  slackUserID: string;
  slackChannelID: string;
  slackChannelName?: string;
  slackTeamID?: string;
  slackTeamDomain: string;
  action?: { type: string; value: { [key: string]: string } };
}

export function reminderToRegisterHoursAndActivateMessage(
  tokenPayload: TokenPayload
) {
  return {
    text: "",
    blocks: [
      hello(tokenPayload.slackUserID),
      {
        ...section(
          "Har du husket å føre timene dine? Trykk på knappen for å hoppe direkte til Alvtime."
        ),
        accessory: alvtimeKnapp(),
      },
      {
        type: "divider",
      },
      {
        type: "context",
        elements: [
          {
            type: "mrkdwn",
            text: "Har du ført timene dine og ønsker kun å få denne beskjeden dersom du glemmer det? Koble Slack-kontoen din til Alvtime via knappen under.",
          },
        ],
      },
      {
        type: "actions",
        elements: [loginButton(tokenPayload)],
      },
    ],
  };
}

export function registerHoursReminderMessage(
  slackUserID: string,
  userReport: UserReport,
  tasks: Task[]
) {
  const weekLogg =
    userReport.sum > 0
      ? [
          section(
            "Dette er timene du så langt har ført for forrige uke :calendar:"
          ),
          ...weekLoggMessage(userReport.entries, tasks),
        ]
      : [];

  return {
    text: "",
    blocks: [
      hello(slackUserID),
      {
        ...section(
          `Har du husket å føre alle timene dine for forrige uke? Jeg har bare fått med meg at du har ført ${userReport.sum} timer. Trykk på knappen for å hoppe til Alvtime og føre resten.`
        ),
        accessory: alvtimeKnapp(),
      },
      ...weekLogg,
    ],
  };
}

export function weekLoggMessage(
  timeEntriesWithValue: { date: string; taskId: number; value: number }[],
  tasks: Task[]
) {
  let blocks: any[] = [];
  const week = createWeek(configuredMoment(timeEntriesWithValue[0].date));
  for (const day of week) {
    const entries = timeEntriesWithValue.filter(
      (entrie) => entrie.date === day.format(config.DATE_FORMAT)
    );

    let text = `*${capitalizeFirstLetter(day.format("dddd D."))}*\n`;
    for (const entrie of entries) {
      const task = tasks.find((task: Task) => task.id === entrie.taskId);

      if (entrie.value !== 0) {
        text =
          text + capitalizeFirstLetter(`${task.name} - \`${entrie.value}\`\n`);
      }
    }
    blocks = [...blocks, section(text)];
  }

  return blocks;
}

export function loggMessage(timeEntries: TimeEntrie[], tasks: Task[]) {
  const timeEntriesWithValue = timeEntries.filter(
    (entrie: TimeEntrie) => entrie.value !== 0
  );

  let message;
  if (timeEntriesWithValue.length === 0) {
    message = {
      text: "Du har ikke ført noen timer denne uken :calendar:",
    };
  } else {
    message = {
      blocks: [
        section("Timer ført denne uken :calendar:"),
        ...weekLoggMessage(timeEntriesWithValue, tasks),
      ],
    };
  }

  return message;
}

export function vacationLoggMessage({
  fromDateInclusive,
  toDateInclusive,
}: DateRange) {
  const fromDate = configuredMoment(fromDateInclusive).format("dddd D. MMMM YYYY");
  const toDate = configuredMoment(toDateInclusive).format("dddd D. MMMM YYYY");

  return { text: `Du har ført ferie fra og med ${fromDate} til og med ${toDate}` };
}

export function loginMessage(tokenPayload: TokenPayload) {
  const { slackUserID } = tokenPayload;

  return {
    text: "",
    blocks: [
      hello(slackUserID),
      {
        ...section(
          "Du må koble Alvtime sammen med Slack. Trykk på knappen og følg instruksjonene. Så vil du kanskje få det du ba om :wink:"
        ),
        accessory: loginButton(tokenPayload),
      },
    ],
  };
}

function alvtimeKnapp() {
  return {
    type: "button",
    action_id: "open_alvtime_button", // We need to add this
    text: {
      type: "plain_text",
      text: "Alvtime",
      emoji: true,
    },
    url: "https://www.alvtime.no",
    value: "alvtime_button_clicked",
  };
}

function hello(slackUserID: string) {
  return section(`Hei <@${slackUserID}> :wave:`);
}

function section(text: string) {
  return {
    type: "section",
    text: {
      type: "mrkdwn",
      text,
    },
  };
}

function loginButton(tokenPayload: TokenPayload) {
  const token = encodePayload(tokenPayload);
  return {
    type: "button",
    action_id: "open_login_in_browser",
    text: {
      type: "plain_text",
      text: "Koble Alvtime til Slack",
      emoji: true,
    },
    url: env.HOST + "/oauth2/login?token=" + token,
    value: "login_button_clicked",
  };
}

function createWeek(day: Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map((n) => monday.clone().add(n, "day"));
}

function encodePayload(info: TokenPayload) {
  const iat = new Date().getTime();
  const exp = iat + 60 * 60 * 1000;
  const payload = {
    ...info,
    iat,
    exp,
  };
  return jwt.encode(payload, config.JWT_SECRET);
}

export function capitalizeFirstLetter(s: string) {
  return s.charAt(0).toUpperCase() + s.slice(1);
}
