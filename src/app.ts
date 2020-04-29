import { createEventAdapter } from "@slack/events-api";
import { createMessageAdapter } from "@slack/interactive-messages";
import bodyParser from "body-parser";
import express from "express";

import dotenv from "dotenv";
if (process.env.NODE_ENV !== "production") {
  dotenv.config({ path: ".env" });
}

import {
  onAppMention,
  onOpenModalButton,
  onCuteAnimalModalSubmit,
} from "./handlers";

const port = process.env.PORT || 3000;
const app = express();

const slackEvents = createEventAdapter(process.env.SLACK_SIGNING_SECRET);
const slackInteractions = createMessageAdapter(
  process.env.SLACK_SIGNING_SECRET
);

app.use("/slack/events", slackEvents.expressMiddleware());
app.use("/slack/actions", slackInteractions.expressMiddleware());

app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

slackEvents.on("app_mention", onAppMention);

slackInteractions.action({ actionId: "open_modal_button" }, onOpenModalButton);

slackInteractions.viewSubmission(
  "cute_animal_modal_submit",
  onCuteAnimalModalSubmit
);

// Starts server
app.listen(port, function () {
  console.log("Bot is listening on port " + port);
});
