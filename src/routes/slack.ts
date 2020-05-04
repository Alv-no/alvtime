import { createEventAdapter } from "@slack/events-api";
import { createMessageAdapter } from "@slack/interactive-messages";
import { WebClient } from "@slack/web-api";
import bodyParser from "body-parser";
import express from "express";
import env from "../environment";

const token = process.env.SLACK_BOT_TOKEN;
export const slackWebClient = new WebClient(token);

const slackRouter = express.Router();

const slackEvents = createEventAdapter(env.SLACK_SIGNING_SECRET);
export const slackInteractions = createMessageAdapter(env.SLACK_SIGNING_SECRET);
slackRouter.use("/events", slackEvents.expressMiddleware());
slackRouter.use("/actions", slackInteractions.expressMiddleware());

slackRouter.use(bodyParser.urlencoded({ extended: true }));
slackRouter.use(bodyParser.json());

interface CommandBody {
  token: string;
  team_id: string;
  team_domain: string;
  channel_id: string;
  channel_name: string;
  user_id: string;
  user_name: string;
  command: string;
  text: string;
  response_url: string;
  trigger_id: string;
}

slackRouter.use("/command", (req, res, next) => {
  console.log(req.body);
  next();
});

slackRouter.use("/command", (req, res, next) => {
  const { text, channel_id, user_name } = req.body;
  if (text === "test") {
    const loginMessage = createLoginMessage(user_name, channel_id);
    slackWebClient.chat.postMessage(loginMessage);
    res.send("");
  }
  next();
});

function createLoginMessage(name: string, channelId: string) {
  return {
    text: "",
    blocks: [
      { type: "section", text: { type: "mrkdwn", text: `Hei ${name} :wave:` } },
      {
        type: "section",
        text: { type: "mrkdwn", text: "Du er ikke logget inn." },
        accessory: {
          type: "button",
          action_id: "open_login_in_browser",
          text: { type: "plain_text", text: "Login med Azure Ad", emoji: true },
          url: env.HOST + "/oauth2/azuread",
          value: "login_button_clicked",
        },
      },
    ],
    channel: channelId,
  };
}

slackRouter.post("/command", (req, res) => {
  const body = req.body as CommandBody;
  slackWebClient.chat.postMessage({ text: "HEI", channel: body.channel_id });
  res.send("");
});

slackInteractions.action(
  { actionId: "open_login_in_browser" },
  async function () {
    return {
      text: "Åpner login nettside...",
    };
  }
);

export default slackRouter;
