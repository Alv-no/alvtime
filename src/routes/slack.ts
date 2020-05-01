import { createEventAdapter } from "@slack/events-api";
import { createMessageAdapter } from "@slack/interactive-messages";
import { WebClient } from "@slack/web-api";
import bodyParser from "body-parser";
import express from "express";
import { messageJsonBlock, modalJsonBlock } from "../blocks";
import env from "../environment";

const token = process.env.SLACK_BOT_TOKEN;
const webClient = new WebClient(token);

const slackRouter = express.Router();

const slackEvents = createEventAdapter(env.SLACK_SIGNING_SECRET);
const slackInteractions = createMessageAdapter(env.SLACK_SIGNING_SECRET);
slackRouter.use("/events", slackEvents.expressMiddleware());
slackRouter.use("/actions", slackInteractions.expressMiddleware());

slackRouter.use(bodyParser.urlencoded({ extended: true }));
slackRouter.use(bodyParser.json());

slackEvents.on("app_mention", async function (event: any) {
  try {
    const mentionResponseBlock = {
      ...messageJsonBlock,
      ...{ channel: event.channel },
    };
    console.log(mentionResponseBlock);
    const res = await webClient.chat.postMessage(mentionResponseBlock);
    console.log("Message sent: ", res.ts);
  } catch (e) {
    console.log(JSON.stringify(e));
  }
});

slackInteractions.action({ actionId: "open_modal_button" }, async function (
  payload: any
) {
  try {
    await webClient.views.open({
      trigger_id: payload.trigger_id,
      view: modalJsonBlock,
    });
  } catch (e) {
    console.log(JSON.stringify(e));
  }

  // The return value is used to update the message where the action occurred immediately.
  // Use this to items like buttons and menus that you only want a user to interact with once.
  return {
    text: "Processing...",
  };
});
slackInteractions.viewSubmission("cute_animal_modal_submit", function (
  payload: any
) {
  const blockData = payload.view.state;

  const cuteAnimalSelection =
    blockData.values.cute_animal_selection_block.cute_animal_selection_element
      .selected_option.value;
  const nameInput =
    blockData.values.cute_animal_name_block.cute_animal_name_element.value;

  console.log(cuteAnimalSelection, nameInput);

  if (nameInput.length < 2) {
    return {
      response_action: "errors",
      errors: {
        cute_animal_name_block:
          "Cute animal names must have more than one letter.",
      },
    };
  }
  return {
    response_action: "clear",
  };
});

export default slackRouter;
