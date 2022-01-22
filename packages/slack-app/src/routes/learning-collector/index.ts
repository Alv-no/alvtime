import { App, ExpressReceiver, SayFn, ViewStateValue } from "@slack/bolt";
import { ChatPostMessageArguments, Logger } from "@slack/web-api";
import express from "express";
import Fuse from "fuse.js";
import { logger } from "../../createLogger";
import env from "../../environment";
import learningDB from "../../models/learnings";
import tagsDB, { Tag } from "../../models/tags";
import { isIm } from "./messageFilters";
import {
  bostAboutLearning,
  IS_LEARNING_BUTTON_CLICKED,
  TAG_BUTTON_CLICKED,
  weekSummary,
  whatUserIsLearningQuestion,
} from "./messages";
import { acknowledge } from "./middleware";
import createModal, {
  COLLECT_LEARNING_MODAL_ID,
  SELECTING_FELLOW_LEARNERS,
  SELECTING_TECH_TAGS_IN_MODAL,
} from "./modal";
import { LearningSummary } from "./models";
import { updateFromCVPartner } from "./tags";

export const FAG_CHANNEL_ID = "C02TUVC9LJ2";
export const FREE_DISTRIBUTION = "FREE_DISTRIBUTION";
export const ONLY_SUMMARY = "ONLY_SUMMARY";
const learningCollector = express.Router();

const boltReceiver = new ExpressReceiver({
  signingSecret: env.LEARNING_COLLECTOR_SLACK_BOT_SIGNING_SECRET,
  endpoints: "/",
});
const boltApp = new App({
  token: env.LEARNING_COLLECTOR_SLACK_BOT_TOKEN,
  receiver: boltReceiver,
});

async function logPayload({ body, logger, payload }: any) {
  logger.info("body");
  logger.info(body);
  logger.info("payload");
  logger.info(payload);
}

boltApp.message(isIm, askForWhatUserIsLearning);
boltApp.event("app_mention", askForWhatUserIsLearning);

async function askForWhatUserIsLearning({
  say,
}: {
  say: SayFn;
  logger: Logger;
}) {
  try {
    say(whatUserIsLearningQuestion());
  } catch (error) {
    logger.error(error);
  }
}

boltApp.action(new RegExp(TAG_BUTTON_CLICKED), acknowledge, async () => {});
boltApp.action(SELECTING_TECH_TAGS_IN_MODAL, acknowledge, logPayload);

boltApp.action(
  IS_LEARNING_BUTTON_CLICKED,
  acknowledge,
  async ({ client, body }) => {
    try {
      await client.views.open(
        //@ts-ignore
        createModal(body.trigger_id, body.user.username, body.user.id)
      );
    } catch (error) {
      logger.error(error);
    }
  }
);

function parseViewStateValues(values: {
  [blockId: string]: {
    [actionId: string]: ViewStateValue;
  };
}) {
  const shareability =
    values.shareability.shareability_radio_buttons_action.selected_option.value;

  const description = values.description.description_input_action.value;

  const locationOfDetails =
    values.locationOfDetails.locationOfDetails_input_action.value;

  const learners = values.learners[SELECTING_FELLOW_LEARNERS].selected_users;

  const selectedTags =
    values.tags[SELECTING_TECH_TAGS_IN_MODAL].selected_options;
  const tags = removeDuplicates(selectedTags.map((option) => option.value));

  return { shareability, description, locationOfDetails, learners, tags };
}

boltApp.view(COLLECT_LEARNING_MODAL_ID, acknowledge, async ({ body, view }) => {
  try {
    const slackUserID = body.user.id;
    const state = parseViewStateValues(view.state.values);
    const { shareability, description, locationOfDetails, learners, tags } =
      state;

    // Make sure the poster is one of the learners
    if (!learners.includes(slackUserID)) learners.push(slackUserID);

    let shareChatPostMessageResponse;
    if (shareability === "all") {
      shareChatPostMessageResponse = await postMessageWithReactions(
        bostAboutLearning(state),
        ["tada", "brain"]
      );
    }
    const thanksChatPostMessageResponse = await postMessageWithReactions(
      {
        channel: body.user.id,
        text: "Takk for at du deler hva du leker med",
      },
      ["tada"]
    );

    const savedTags = await tagsDB.getAll();
    const newTags = tags.filter(
      (tag) => !savedTags.some((savedTag) => savedTag.term === tag)
    );
    for (const tag of newTags) {
      await tagsDB.save({ term: tag });
    }

    await learningDB.save({
      createdAt: new Date(),
      description,
      locationOfDetails,
      shareMessage: shareChatPostMessageResponse
        ? {
            channel: shareChatPostMessageResponse.channel,
            timestamp: shareChatPostMessageResponse.ts,
          }
        : undefined,
      thanksMessage: {
        channel: thanksChatPostMessageResponse.channel,
        timestamp: thanksChatPostMessageResponse.ts,
      },
      shareability,
      slackUserID,
      learners,
      tags,
    });
  } catch (error) {
    logger.error(error);
  }
});

async function postMessageWithReactions(
  message: ChatPostMessageArguments,
  reactions: string[]
) {
  const client = boltApp.client;
  const chatPostMessageResponse = await client.chat.postMessage(message);
  for (let reaction of reactions) {
    client.reactions.add({
      channel: chatPostMessageResponse.channel,
      timestamp: chatPostMessageResponse.ts,
      name: reaction,
    });
  }
  return chatPostMessageResponse;
}

boltApp.command("/lærer", acknowledge, async ({ body, client, payload }) => {
  const postSummary = payload.text.includes("summary");
  const runUpdateFromCVPartner = payload.text.includes("cv");
  try {
    const { members } = await client.users.list();
    if (runUpdateFromCVPartner) {
      await updateFromCVPartner();
    } else if (postSummary) {
      const learnings = await learningDB.findCreatedAfter(
        new Date(2022, 0, 11)
      );
      let learningSummary: LearningSummary[] = [];
      for (const learning of learnings) {
        const {
          createdAt,
          description,
          learners,
          locationOfDetails,
          shareMessage,
          shareMessage: { channel, timestamp },
          shareability,
          slackUserID,
          thanksMessage,
          tags,
        } = learning;
        const {
          message: { reactions },
        } = await client.reactions.get({
          channel,
          timestamp,
          full: true,
        });
        const member = members.find((member) => member.id === slackUserID);
        learningSummary.push({
          createdAt,
          description,
          learners,
          locationOfDetails,
          member,
          reactions,
          shareMessage,
          shareability,
          slackUserID,
          thanksMessage,
          tags,
        });
      }
      const weekSum = weekSummary(learningSummary);
      logger.error(weekSum);
      await client.chat.postMessage(weekSum);
    } else {
      const result = await client.views.open(
        //@ts-ignore
        createModal(body.trigger_id, payload.user_name, payload.user_id)
      );
      logger.info(result);
    }
  } catch (error) {
    logger.error(error);
  }
});

learningCollector.use("/events", boltReceiver.router);
learningCollector.use("/events", boltReceiver.app);

learningCollector.post("/tags", express.urlencoded(), async (req, res) => {
  const payload = JSON.parse(req.body.payload);
  const pattern = payload.value;
  const savedTags = await tagsDB.getAll();
  const searchResult = new Fuse(savedTags, { keys: ["term"] }).search(pattern);
  const options = searchResult.map(fuseOutputToOption).slice(0, 7);
  const isExactMatch = options.some(
    (option) => option.text.text.toLowerCase() === pattern.toLowerCase()
  );
  if (!isExactMatch) options.push(createToBeAddedOption(pattern));
  res.json({ options });
});

export default learningCollector;

function createToBeAddedOption(name: string) {
  return {
    text: { type: "plain_text", text: `"${name}" (ny tag)` },
    value: name,
  };
}

function fuseOutputToOption(fuseOutput: { item: Tag }) {
  const name = fuseOutput.item.term;
  return {
    text: { type: "plain_text", text: name },
    value: name,
  };
}

function removeDuplicates(arr: string[]) {
  return [...new Set(arr)];
}
