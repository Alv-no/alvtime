import express from "express";
import { loginMessage } from "../../messages/index";
import userDB from "../../models/user";
import { slackInteractions } from "./index";
import runCommand from "./runCommand";
import respondToResponseURL from "../../response/respondToResponseURL";

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
  const user = await userDB.findById(req.body.user_id);
  if (!user) {
    const loginPayload = {
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

    respondToResponseURL(req.body.response_url, loginMessage(loginPayload));
    res.send("");
  } else {
    next();
  }
}
