import express from "express";
import env from "../../environment";
import { slackWebClient, slackInteractions } from "./index";

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

function createSlashCommandRouter() {
  const slashCommandRouter = express.Router();

  slashCommandRouter.use("/", (req, _res, next) => {
    console.log(req.body);
    next();
  });

  slashCommandRouter.use("/", (req, _res, next) => {
    const { text, channel_id, user_name } = req.body;
    if (text === "test") {
      const loginMessage = createLoginMessage(user_name, channel_id);
      slackWebClient.chat.postMessage(loginMessage);
    }
    next();
  });

  slashCommandRouter.post("/", (req, res) => {
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

  return slashCommandRouter;
}

function createLoginMessage(name: string, channelId: string) {
  return {
    text: "",
    blocks: [
      {
        type: "section",
        text: { type: "mrkdwn", text: `Hei ${name} :wave:` },
      },
      {
        type: "section",
        text: { type: "mrkdwn", text: "Du er ikke logget inn." },
        accessory: {
          type: "button",
          action_id: "open_login_in_browser",
          text: {
            type: "plain_text",
            text: "Login med Azure Ad",
            emoji: true,
          },
          url: env.HOST + "/oauth2/azuread",
          value: "login_button_clicked",
        },
      },
    ],
    channel: channelId,
  };
}

export default createSlashCommandRouter;
