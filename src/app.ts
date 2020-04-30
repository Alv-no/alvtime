import { createEventAdapter } from "@slack/events-api";
import { createMessageAdapter } from "@slack/interactive-messages";
import bodyParser from "body-parser";
import express from "express";
import {
  onAppMention,
  onCuteAnimalModalSubmit,
  onOpenModalButton,
} from "./handlers";
import oauth2 from "./routes/oauth2";

const app = express();

const slackEvents = createEventAdapter(process.env.SLACK_SIGNING_SECRET);
const slackInteractions = createMessageAdapter(
  process.env.SLACK_SIGNING_SECRET
);

app.use("/slack/events", slackEvents.expressMiddleware());
app.use("/slack/actions", slackInteractions.expressMiddleware());

app.use(bodyParser.urlencoded({ extended: true }));
app.use(bodyParser.json());

app.use("/oauth2", oauth2);

slackEvents.on("app_mention", onAppMention);

slackInteractions.action({ actionId: "open_modal_button" }, onOpenModalButton);

slackInteractions.viewSubmission(
  "cute_animal_modal_submit",
  onCuteAnimalModalSubmit
);

// Starts server
const port = process.env.PORT || 3000;
app.listen(port, function () {
  console.log("Alvtime slack app is listening on port " + port);
});
