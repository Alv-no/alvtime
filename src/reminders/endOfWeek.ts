import cron from "node-cron";
import { slackInteractions, slackWebClient } from "../routes/slack";
import { capitalizeFirstLetter } from "../utils/text";

interface Member {
  id: string;
  name: string;
  deleted: boolean;
  profile: {
    email: string;
  };
  is_bot: boolean;
}

const fridays1630 = "30 14 * * 5"; // UTC time
const thursdays1900 = "0 17 * * 4"; // UTC time

function startEndOfWeekReminder() {
  cron.schedule(thursdays1900, remindUsersToRegisterHours);

  slackInteractions.action(
    { actionId: "open_alvtime_button" },
    async function () {
      return {
        text: "Åpner Alvtime...",
      };
    }
  );
}

async function remindUsersToRegisterHours() {
  try {
    const users = await getUsers();
    const user = users[0];
    sendReminder(user);
  } catch (error) {
    console.error(error);
  }
}

async function getUsers() {
  const res = ((await slackWebClient.users.list()) as unknown) as {
    members: Member[];
  };

  const users = res.members.filter(
    (member) =>
      !member.is_bot && member.id !== "USLACKBOT" && member.id === "UR64B2VGE"
  );

  return users;
}

async function sendReminder(user: Member) {
  const converstion = ((await slackWebClient.conversations.open({
    users: user.id,
  })) as unknown) as { channel: { id: string } };

  slackWebClient.chat.postMessage({
    ...createRegisterHoursReminder(user.name),
    channel: converstion.channel.id,
  });
}

const createRegisterHoursReminder = (name: string) => {
  name = capitalizeFirstLetter(name);

  return {
    text: "",
    blocks: [
      {
        type: "section",
        text: {
          type: "mrkdwn",
          text: `Hei ${name} :wave:`,
        },
      },
      {
        type: "section",
        text: {
          type: "mrkdwn",
          text: "Har du husket å føre timene dine?",
        },
        accessory: {
          type: "button",
          action_id: "open_alvtime_button", // We need to add this
          text: {
            type: "plain_text",
            text: "Alvtime",
            emoji: true,
          },
          url: "https://www.alvtime.no",
          value: "alvtime_button_clicked",
        },
      },
    ],
  };
};

export default startEndOfWeekReminder;
