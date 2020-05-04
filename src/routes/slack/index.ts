import { createEventAdapter } from "@slack/events-api";
import { createMessageAdapter } from "@slack/interactive-messages";
import { WebClient } from "@slack/web-api";
import bodyParser from "body-parser";
import express from "express";
import env from "../../environment";
import createSlashCommandRouter from "./slashCommand";

const token = process.env.SLACK_BOT_TOKEN;
export const slackWebClient = new WebClient(token);

const slackRouter = express.Router();

const slackEvents = createEventAdapter(env.SLACK_SIGNING_SECRET);
export const slackInteractions = createMessageAdapter(env.SLACK_SIGNING_SECRET);
slackRouter.use("/events", slackEvents.expressMiddleware());
slackRouter.use("/actions", slackInteractions.expressMiddleware());

slackRouter.use(bodyParser.urlencoded({ extended: true }));
slackRouter.use(bodyParser.json());

slackRouter.use("/command", createSlashCommandRouter());

export default slackRouter;
