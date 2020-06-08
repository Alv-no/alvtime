import jwt from "jwt-simple";
import { Moment } from "moment";
import { Task } from "../client/index";
import config from "../config";
import env from "../environment";
import configuredMoment from "../moment";
import { UserReport } from "../reminders/lastWeeksHours";
import { capitalizeFirstLetter } from "../utils/text";

export interface TokenPayload {
  slackUserName: string;
  slackUserID: string;
  slackChannelID: string;
  slackChannelName?: string;
  slackTeamID?: string;
  slackTeamDomain: string;
  action?: { type: string; value: { [key: string]: string } };
}

const alvtimeKnapp = {
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

function createHello(slackUserID: string) {
  return {
    type: "section",
    text: {
      type: "mrkdwn",
      text: `Hei <@${slackUserID}> :wave:`,
    },
  };
}

export const createReminderToRegisterHoursAndActivate = (
  tokenPayload: TokenPayload
) => {
  return {
    text: "",
    blocks: [
      createHello(tokenPayload.slackUserID),
      {
        type: "section",
        text: {
          type: "mrkdwn",
          text:
            "Har du husket å føre timene dine? Trykk på knappen for å hoppe direkte til Alvtime.",
        },
        accessory: alvtimeKnapp,
      },
      {
        type: "divider",
      },
      {
        type: "context",
        elements: [
          {
            type: "mrkdwn",
            text:
              "Har du ført timene dine og ønsker kun å få denne beskjeden dersom du glemmer det? Koble Slack-kontoen din til Alvtime via knappen under.",
          },
        ],
      },
      {
        type: "actions",
        elements: [createLoginButton(tokenPayload)],
      },
    ],
  };
};

export const createRegisterHoursReminder = (
  slackUserID: string,
  userReport: UserReport,
  tasks: Task[]
) => {
  const weekLogg =
    userReport.sum > 0
      ? [
          {
            type: "section",
            text: {
              type: "mrkdwn",
              text:
                "Dette er timene du så langt har ført for forrige uke :calendar:",
            },
          },
          ...createWeekLogg(userReport.entries, tasks),
        ]
      : [];

  return {
    text: "",
    blocks: [
      createHello(slackUserID),
      {
        type: "section",
        text: {
          type: "mrkdwn",
          text: `Har du husket å føre alle timene dine for forrige uke? Jeg har bare fått med meg at du har ført ${userReport.sum} timer. Trykk på knappen for å hoppe til Alvtime og føre resten.`,
        },
        accessory: alvtimeKnapp,
      },
      ...weekLogg,
    ],
  };
};

export function createWeekLogg(
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
    blocks = [
      ...blocks,
      {
        type: "section",
        text: {
          type: "mrkdwn",
          text,
        },
      },
    ];
  }

  return blocks;
}

function createWeek(day: Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map((n) => monday.clone().add(n, "day"));
}

function createToken(info: TokenPayload) {
  const iat = new Date().getTime();
  const exp = iat + 60 * 60 * 1000;
  const payload = {
    ...info,
    iat,
    exp,
  };
  return jwt.encode(payload, config.JWT_SECRET);
}

export function createLoginMessage(tokenPayload: TokenPayload) {
  const { slackUserID } = tokenPayload;

  return {
    text: "",
    blocks: [
      createHello(slackUserID),
      {
        type: "section",
        text: {
          type: "mrkdwn",
          text:
            "Du har ikke koblet Alvtime sammen med Slack enda. Trykk på knappen under og følg instruksjonene. Så vil du kanskje få det du ba om :wink:",
        },
        accessory: createLoginButton(tokenPayload),
      },
    ],
  };
}

function createLoginButton(tokenPayload: TokenPayload) {
  const token = createToken(tokenPayload);
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
