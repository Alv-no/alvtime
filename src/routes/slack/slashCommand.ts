import express from "express";
import jwt from "jwt-simple";
import config from "../../config";
import env from "../../environment";
import UserModel from "../../models/user";
import { capitalizeFirstLetter } from "../../utils/text";
import { slackInteractions } from "./index";
import runCommand from "./runCommand";
import sendCommandResponse from "./sendCommandResponse";

export interface CommandBody {
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

export interface LoginInfo {
  slackUserName: string;
  slackUserID: string;
  slackChannelID: string;
  slackChannelName: string;
  slackTeamID: string;
  slackTeamDomain: string;
  action: { type: string; value: { [key: string]: string } };
}

export interface LoginTokenData extends LoginInfo {
  exp: number;
  iat: number;
}

export const actionTypes = Object.freeze({
  COMMAND: "COMMAND",
});

export default function createSlashCommandRouter() {
  const slashCommandRouter = express.Router();

  slashCommandRouter.use("/", authenticate);

  slashCommandRouter.post("/", (req, res) => {
    const commandBody = req.body as CommandBody;
    runCommand(commandBody);
    res.send("");
  });

  slackInteractions.action({ actionId: "open_login_in_browser" }, () => ({
    text: "Åpner login nettside...",
  }));

  return slashCommandRouter;
}

async function authenticate(
  req: { body: CommandBody },
  res: { send: (s: string) => void },
  next: (s?: string) => void
) {
  const user = await UserModel.findById(req.body.user_id);
  if (!user) {
    const loginInfo = {
      slackUserName: req.body.user_name,
      slackUserID: req.body.user_id,
      slackChannelID: req.body.channel_id,
      slackChannelName: req.body.channel_name,
      slackTeamID: req.body.team_id,
      slackTeamDomain: req.body.team_domain,
      action: {
        type: actionTypes.COMMAND,
        value: (req.body as unknown) as { [key: string]: string },
      },
    };
    sendLoginMessage(loginInfo);
    res.send("");
  } else {
    next();
  }
}

function sendLoginMessage(info: LoginInfo) {
  const { slackUserName, slackChannelID, action } = info;
  const token = createToken(info);
  const loginMessage = createLoginMessage(slackUserName, slackChannelID, token);
  sendCommandResponse(action.value.response_url, loginMessage);
}

function createToken(info: LoginInfo) {
  const iat = new Date().getTime();
  const exp = iat + 60 * 60 * 1000;
  const payload = {
    ...info,
    iat,
    exp,
  };
  return jwt.encode(payload, config.JWT_SECRET);
}

function createLoginMessage(name: string, channelId: string, token: string) {
  name = capitalizeFirstLetter(name);
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
            text: "Connect Alvtime account",
            emoji: true,
          },
          url: env.HOST + "/oauth2/login?token=" + token,
          value: "login_button_clicked",
        },
      },
    ],
    channel: channelId,
  };
}
