import express from "express";
import env from "../../environment";
import { logger } from "../../createLogger";
import { App, ExpressReceiver } from "@slack/bolt";

const learningCollector = express.Router();

const boltReceiver = new ExpressReceiver({
  signingSecret: env.LEARNING_COLLECTOR_SLACK_BOT_SIGNING_SECRET,
  endpoints: "/",
});
const boltApp = new App({
  token: env.LEARNING_COLLECTOR_SLACK_BOT_TOKEN,
  receiver: boltReceiver,
});

boltApp.event("app_mention", async ({ event }) => logger.info(event));
boltApp.action("button_abc", async (event) => {
  event.logger.info(event.body);
  await event.ack();
  // Update the message to reflect the action
});

boltApp.command("/lærer", async ({ ack, body, client, logger }) => {
  // Acknowledge the command request
  await ack();

  try {
    // Call views.open with the built-in client
    const result = await client.views.open({
      // Pass a valid trigger_id within 3 seconds of receiving it
      trigger_id: body.trigger_id,
      // View payload
      view: {
        type: "modal",
        // View identifier
        callback_id: "view_1",
        title: {
          type: "plain_text",
          text: "Modal title",
        },
        blocks: [
          {
            type: "section",
            text: {
              type: "mrkdwn",
              text: "Welcome to a modal with _blocks_",
            },
            accessory: {
              type: "button",
              text: {
                type: "plain_text",
                text: "Click me!",
              },
              action_id: "button_abc",
            },
          },
          {
            type: "input",
            block_id: "input_c",
            label: {
              type: "plain_text",
              text: "What are your hopes and dreams?",
            },
            element: {
              type: "plain_text_input",
              action_id: "dreamy_input",
              multiline: true,
            },
          },
        ],
        submit: {
          type: "plain_text",
          text: "Submit",
        },
      },
    });
    logger.info(result);
  } catch (error) {
    logger.error(error);
  }
});

learningCollector.use("/events", boltReceiver.router);

export default learningCollector;
