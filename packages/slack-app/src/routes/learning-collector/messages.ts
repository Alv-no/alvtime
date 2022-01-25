import { Block, KnownBlock } from "@slack/bolt";
import { Member } from "@slack/web-api/dist/response/UsersListResponse";
import { logger } from "../../createLogger";
import { boltApp, LEARNING_COLLECTOR_SHARING_CHANNEL_ID } from ".";
import { markdown, plainText } from "./blocks";
import { LearningSummary } from "./models";
import { getResponseMessage } from "./responses";
import { getRandomNumber, getReactionsFromMessage } from "./reactions";
import { Learning } from "../../models/learnings";
import { Reaction } from "@slack/web-api/dist/response/ChannelsHistoryResponse";

type Blocks = (KnownBlock | Block)[];

export function whatUserIsLearningQuestion() {
  return {
    blocks: [
      {
        type: "section",
        text: markdown(
          getResponseMessage("whatUserIsLearningQuestion")
        ),
      },
      openModalButton(),
    ],
  };
}

type Learning = {
  description: string;
  locationOfDetails: string;
  learners: string[];
  tags: string[];
};

export function boastAboutLearning(state: Learning) {
  const { description, locationOfDetails, learners, tags } = state;
  const usersText = createMultipleUsersText(learners);
  const moreInfoText = !!locationOfDetails
    ? `\n${getResponseMessage("learnMoreText")} :point_down:\n${locationOfDetails}`
    : "";
  const text =
    `${usersText}  ${getResponseMessage("boastAboutLearningText")}\n\n>${description}` +
    moreInfoText;
  const blocks: Blocks = [
    {
      type: "section",
      text: markdown(text),
    },
  ];
  if (tags?.length) blocks.push(createTagButtons(tags));
  blocks.push({ type: "divider" });
  blocks.push(openModalButton());

  return {
    channel: LEARNING_COLLECTOR_SHARING_CHANNEL_ID,
    text: `${usersText} ${getResponseMessage("boastAboutLearningText")}\n\n>${description}\n${!!locationOfDetails ? getResponseMessage("learnMoreText") + ` \n${locationOfDetails}` : ""}`,
    blocks
  };
}

export function informLearnerAboutRegistration(
  registrar: string,
  learning: Learning
) {
  const { description, locationOfDetails, tags } = learning;
  const moreInfoText = locationOfDetails
    ? `\nFinn mere info :point_down:\n${locationOfDetails}`
    : "";
  const blocks: Blocks = [
    {
      type: "section",
      text: markdown(
        `<@${registrar}> fortalte meg akkurat at dere har lært om :point_down:\n\n>${description}` +
          moreInfoText
      ),
    },
  ];
  if (tags?.length) blocks.push(createTagButtons(tags));
  blocks.push({ type: "divider" });
  blocks.push(openModalButton());

  return {
    text: `<@${registrar}> fortalte meg akkurat at dere har lært om ...`,
    blocks,
  };
}

export function thankYouForSharing() {
  return { text: "Takk for at du deler hva du leker med" };
}

function createMultipleUsersText(userIds: string[]) {
  return userIds
    .map((learner, index, arr) => {
      if (index === 0) return `<@${learner}>`;
      if (index === arr.length - 1) return ` og <@${learner}>`;
      return `, <@${learner}>`;
    })
    .join("");
}

export const IS_LEARNING_BUTTON_CLICKED = "is_learning_button_clicked";
function openModalButton(text = "Fortell hva du lærer!") {
  return {
    type: "actions",
    elements: [
      {
        type: "button",
        text: plainText(text),
        style: "primary",
        action_id: IS_LEARNING_BUTTON_CLICKED,
      },
    ],
  };
}

async function createWeekRepport(learnings: Learning[], members: Member[]) {
  const learningSummary: LearningSummary[] = [];
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

    const reactions = await getReactionsFromMessage({ channel, timestamp });

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
  return learningSummary;
}

export const TAG_BUTTON_CLICKED = "tag_button_clicked";
export async function weekSummary(learnings: Learning[], members: Member[]) {
  const weekRepport = await createWeekRepport(learnings, members);
  const blocks: Blocks = [
    {
      type: "section",
      text: markdown(
        `*Ukens oppsummering*\nDenne uken har det blitt registrert *${learnings.length}* læringsaktiviteter. Se hva alle de flinke Alvene lærer seg`
      ),
    },
    {
      type: "divider",
    },
    ...weekRepport
      .map(
        ({
          slackUserID,
          description,
          locationOfDetails,
          reactions,
          member,
          tags,
        }) => {
          const reactionsText = createReactionsText(reactions);
          const blocks: Blocks = [
            {
              type: "section",
              text: markdown(
                `*<@${slackUserID}>*\n${reactionsText}\n${description}\nMore info: ${locationOfDetails}`
              ),
              accessory: profilePhoto(member),
            },
          ];
          if (tags?.length) blocks.push(createTagButtons(tags));
          return blocks;
        }
      )
      .flat(),
    { type: "divider" },
    openModalButton(),
  ];

  return {
    channel: LEARNING_COLLECTOR_SHARING_CHANNEL_ID,
    text: "Alt det Alvene har lært denne uken",
    blocks,
  };
}

function createReactionsText(reactions: Reaction[]) {
  return reactions.map(({ name, count }) => `:${name}: ${count}`).join("  ");
}

export async function learnerSummary(learnings: Learning[], members: Member[]) {
  const allLearners = getLearnersFromLearnings(learnings);
  const learningsGroupedByUser = groupLearningsByUser(learnings, allLearners);
  const blocks: Blocks = [
    {
      type: "section",
      text: markdown(
        `*Ukens oppsummering*\nDenne uken har det blitt registrert *${learnings.length}* læringsaktiviteter. Se hva alle de flinke Alvene lærer seg`
      ),
    },
    {
      type: "divider",
    },
  ];
  const userSections: Block[] = [];
  const createUserSection = createUserSectionCreator(
    members,
    learningsGroupedByUser
  );
  for (const userId of Object.values(allLearners)) {
    userSections.push(await createUserSection(userId));
  }
  blocks.push(...userSections);
  blocks.push(openModalButton());

  return {
    channel: LEARNING_COLLECTOR_SHARING_CHANNEL_ID,
    text: "Alt det Alvene har lært denne uken",
    blocks,
  };
}

function createUserSectionCreator(
  members: Member[],
  learningsGroupedByUser: LearningsGroupedByUser
) {
  return async (userId: string) => {
    const member = members.find((m) => m.id === userId);
    let userLearnings = "";
    let index = 0;
    for (const learning of learningsGroupedByUser[userId]) {
      userLearnings =
        userLearnings + (await createLearningSummary(learning, index));
      index++;
    }
    return {
      type: "section",
      text: markdown(userLearnings),
      accessory: profilePhoto(member),
    };
  };
}

function getLearnersFromLearnings(learnings: Learning[]) {
  const allLearners: string[] = [];
  for (const learning of learnings) {
    const newLearnersFromLearning = learning.learners.filter(
      (userId) => allLearners.indexOf(userId) === -1
    );
    allLearners.push(...newLearnersFromLearning);
  }
  return allLearners;
}

type LearningsGroupedByUser = {
  [userId: string]: Learning[];
};

function groupLearningsByUser(learnings: Learning[], allLearners: string[]) {
  const learningsGroupedByUser: LearningsGroupedByUser = {};
  for (const userId of allLearners) {
    const userLearnings = learnings.filter(
      (learning) => learning.learners.indexOf(userId) > -1
    );
    if (!learningsGroupedByUser[userId])
      learningsGroupedByUser[userId] = userLearnings;
  }
  return learningsGroupedByUser;
}

async function createLearningSummary(learning: Learning, index: number) {
  const { description, locationOfDetails, shareMessage } = learning;
  const reactions = await getReactionsFromMessage(shareMessage);
  const reactionsText = createReactionsText(reactions);
  const moreInfoText = locationOfDetails
    ? ` :point_right: ${locationOfDetails}`
    : "";
  return `\n*${index + 1}.* ${description}${moreInfoText}\n${reactionsText}`;
}

function profilePhoto(member: Member) {
  const image_url = member.profile.image_512;
  return {
    type: "image",
    image_url,
    alt_text: `Profile photo of ${member.name}`,
  };
}

function createTagButtons(tags: string[]) {
  return {
    type: "actions",
    elements: tags.map((tag) => ({
      type: "button",
      text: plainText(tag),
      value: tag,
      action_id: `${TAG_BUTTON_CLICKED}-${tag}`,
      url: `https://www.google.com/search?q=${tag}`,
    })),
  };
}
