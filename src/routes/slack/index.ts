import { createEventAdapter } from "@slack/events-api";
import { createMessageAdapter } from "@slack/interactive-messages";
import { WebClient } from "@slack/web-api";
import bodyParser from "body-parser";
import crypto from "crypto";
import express from "express";
import { IncomingMessage, ServerResponse } from "http";
import env from "../../environment";
import createSlashCommandRouter from "./slashCommand";

export const slackWebClient = new WebClient(env.SLACK_BOT_TOKEN);

const slackRouter = express.Router();

const slackEvents = createEventAdapter(env.SLACK_SIGNING_SECRET);
export const slackInteractions = createMessageAdapter(env.SLACK_SIGNING_SECRET);
slackRouter.use("/events", slackEvents.expressMiddleware());
slackRouter.use("/actions", slackInteractions.expressMiddleware());

slackRouter.use(
  "/command",
  bodyParser.urlencoded({
    extended: true,
    verify: verifySlackRequest,
  })
);

slackRouter.use("/command", createSlashCommandRouter());

function verifySlackRequest(
  req: IncomingMessage,
  res: ServerResponse,
  buf: Buffer,
  encoding: string
) {
  const xSlackRequestTimestamp = parseInt(
    req.headers["x-slack-request-timestamp"] as string
  );

  verifyTimeStamp(xSlackRequestTimestamp);

  const slackSignature = req.headers["x-slack-signature"] as string;
  const version = slackSignature.split("=")[0];
  const textBody = buf.toString("utf8");
  const signatureBasestring = `${version}:${xSlackRequestTimestamp}:${textBody}`;
  const sha = generateSha(signatureBasestring);
  const mySignature = version + "=" + sha;

  if (mySignature !== slackSignature) {
    throw "Signature mismatch";
  }
}

function verifyTimeStamp(xSlackRequestTimestamp: number) {
  if (isNaN(xSlackRequestTimestamp)) {
    throw "x-slack-request-timestamp is not number";
  }
  const SIXTY_SECOUNDS = 60;
  const now = Math.floor(new Date().getTime() / 1000);
  const timeDiff = now - xSlackRequestTimestamp;
  if (timeDiff > SIXTY_SECOUNDS) {
    throw "Request is more than 60 secounds old";
  }
}

function generateSha(signatureBasestring: string) {
  const hmac = crypto.createHmac("sha256", env.SLACK_SIGNING_SECRET);
  const hex = hmac.update(signatureBasestring).digest("hex");
  return hex;
}

export default slackRouter;
