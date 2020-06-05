import express from "express";
import session from "express-session";
import jwt from "jwt-simple";
import passport from "passport";
import config from "../../config";
import UserModel from "../../models/user";
import runCommand from "../slack/runCommand";
import sendCommandResponse from "../slack/sendCommandResponse";
import { actionTypes, LoginTokenData } from "../slack/slashCommand";
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
  const loginInfo = jwt.decode(req.query.token, config.JWT_SECRET);
  validateTokenExp(loginInfo.exp);
  req.session.loginInfo = loginInfo;
  res.send(createLoginPage(loginInfo.slackTeamDomain));
});

oauth2Router.get("/azuread", passport.authenticate("azureAd"));

oauth2Router.get(
  "/cb",
  passport.authenticate("azureAd", {
    failureRedirect: "/something-went-wrong",
  }),
  async (req, res, next) => {
    const { slackTeamDomain, slackChannelID } = req.session.loginInfo;
    const written = await writeUserToDb(
      req.user as AuthenticatedUser,
      req.session.loginInfo
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
    } = req.session.loginInfo;

    const { email } = req.user as AuthenticatedUser;

    const loginSuccessMessage = createLoginSuccessMessage(
      slackUserID,
      email,
      slackChannelID
    );
    if (actionTypes.COMMAND === action.type) runCommand(action.value);
    sendCommandResponse(action.value.response_url, loginSuccessMessage);
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

async function writeUserToDb(
  authenticatedUser: AuthenticatedUser,
  loginInfo: LoginTokenData
) {
  let written = false;
  const { slackUserName, slackUserID } = loginInfo;

  const user = await UserModel.findById(slackUserID);
  if (!user) {
    const { name, email, auth } = authenticatedUser;
    const doc = {
      _id: slackUserID,
      name,
      email,
      slackUserName,
      slackUserID,
      auth,
    };

    const user = new UserModel(doc);
    await user.save();
    written = true;
  }
  return written;
}
