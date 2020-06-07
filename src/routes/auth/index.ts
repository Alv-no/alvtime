import express from "express";
import session from "express-session";
import jwt from "jwt-simple";
import passport from "passport";
import config from "../../config";
import { TokenPayload } from "../../messages/index";
import userDB from "../../models/user";
import { slackWebClient } from "../../routes/slack";
import runCommand from "../slack/runCommand";
import sendCommandResponse from "../slack/sendCommandResponse";
import { actionTypes } from "../slack/slashCommand";
import azureAdStrategy, { AuthenticatedUser, DoneFunc } from "./azureAd";
import createLoginPage from "./loginPage";

passport.use("azureAd", azureAdStrategy);
passport.serializeUser((_user: AuthenticatedUser, done: DoneFunc) =>
  done(null, "not in use")
);

const oauth2Router = express.Router();
export default oauth2Router;

oauth2Router.use(passport.initialize());
oauth2Router.use(passport.session());
oauth2Router.use(
  session({
    secret: config.SESSION_SECRET,
    resave: true,
    saveUninitialized: true,
  })
);

oauth2Router.get("/login", (req, res) => {
  const loginPayload = jwt.decode(req.query.token, config.JWT_SECRET);
  validateTokenExp(loginPayload.exp);
  req.session.loginPayload = loginPayload;
  res.send(createLoginPage(loginPayload.slackTeamDomain));
});

oauth2Router.get("/azuread", passport.authenticate("azureAd"));

oauth2Router.get(
  "/cb",
  passport.authenticate("azureAd", {
    failureRedirect: "/something-went-wrong",
  }),
  async (req, res, next) => {
    const { slackTeamDomain, slackChannelID } = req.session.loginPayload;
    const written = await writeUserToDb(
      req.user as AuthenticatedUser,
      req.session.loginPayload
    );
    if (written) {
      next();
    } else {
      res.redirect(
        `https://${slackTeamDomain}.slack.com/app_redirect?channel=${slackChannelID}`
      );
    }
  },
  (req, res) => {
    const {
      slackTeamDomain,
      slackChannelID,
      slackUserID,
      action,
    } = req.session.loginPayload;

    const { email } = req.user as AuthenticatedUser;

    const loginSuccessMessage = createLoginSuccessMessage(
      slackUserID,
      email,
      slackChannelID
    );
    if (action && actionTypes.COMMAND === action.type) {
      runCommand(action.value);
      sendCommandResponse(action.value.response_url, loginSuccessMessage);
    } else {
      slackWebClient.chat.postMessage({
        ...loginSuccessMessage,
        channel: slackChannelID,
      });
    }
    res.redirect(
      `https://${slackTeamDomain}.slack.com/app_redirect?channel=${slackChannelID}`
    );
  }
);

function validateTokenExp(exp: number) {
  const now = new Date().getTime();
  if (now > exp) {
    throw "Expired";
  }
}

function createLoginSuccessMessage(
  slackUserID: string,
  alvtimeUserName: string,
  channelId: string
) {
  return {
    text: "",
    blocks: [
      {
        type: "section",
        text: {
          type: "mrkdwn",
          text: `:white_check_mark: Success! <@${slackUserID}> is now connected to ${alvtimeUserName}`,
        },
      },
    ],
    channel: channelId,
  };
}

export interface LoginTokenData extends TokenPayload {
  exp: number;
  iat: number;
}

async function writeUserToDb(
  authenticatedUser: AuthenticatedUser,
  loginPayload: LoginTokenData
) {
  let written = false;
  const { slackUserName, slackUserID } = loginPayload;

  const user = await userDB.findById(slackUserID);
  if (!user) {
    const { name, email, auth } = authenticatedUser;
    const doc = {
      name,
      email: email.toLowerCase(),
      slackUserName,
      slackUserID,
      auth,
    };

    written = await userDB.save(doc);
  }
  return written;
}
