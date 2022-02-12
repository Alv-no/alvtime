import { Reaction } from "@slack/web-api/dist/response/ChannelsHistoryResponse";
import { Member } from "@slack/web-api/dist/response/UsersListResponse";
import { formatDistance } from "date-fns";
import { nb } from "date-fns/locale";
import { LEARNING_COLLECTOR_SHARING_CHANNEL_ID } from ".";
import { Learning } from "../../models/learnings";
import { markdown, plainText } from "./blocks";
import { Blocks, LearningSummary } from "./models";
import { getReactionsFromMessage, getVoteReactions } from "./reactions";
import { getResponseMessage, thanksForSharingText } from "./responses";

export function whatUserIsLearningQuestion() {
  return {
    blocks: [
      {
        type: "section",
        text: markdown(getResponseMessage("whatUserIsLearningQuestion")),
      },
      openModalButton(),
    ],
  };
}

interface LearningRegistration {
  shareability: string;
  description: string;
  locationOfDetails: string;
  learners: string[];
  tags: string[];
}

export function boastAboutLearning(state: LearningRegistration) {
  const { description, locationOfDetails, learners, tags } = state;
  const usersText = createMultipleUsersText(learners);
  const moreInfoText = getMoreInfoText(locationOfDetails);
  const text = `${usersText}  ${getResponseMessage(
    "boastAboutLearningText"
  )}\n\n>${description}\n\n${moreInfoText}`;
  const blocks: Blocks = [
    {
      type: "section",
      text: markdown(text),
    },
  ];
  if (tags?.length) blocks.push(createTagSection(tags));
  blocks.push({ type: "divider" });
  blocks.push(createFeedbackReactionInstructions());

  return {
    channel: LEARNING_COLLECTOR_SHARING_CHANNEL_ID,
    text,
    blocks,
  };
}

function getMoreInfoText(locationOfDetails: string) {
  return !!locationOfDetails
    ? `${getResponseMessage("learnMoreText")}  ${locationOfDetails}`
    : "";
}

function createFeedbackReactionInstructions() {
  const texts = getVoteReactions().voteReactionsTexts.join("\n");
  return {
    type: "context",
    elements: [markdown(texts)],
  };
}

export function informLearnerAboutRegistration(
  registrar: string,
  learning: LearningRegistration
) {
  const { description, locationOfDetails, tags } = learning;
  const moreInfoText = getMoreInfoText(locationOfDetails);
  const blocks: Blocks = [
    {
      type: "section",
      text: markdown(
        `<@${registrar}> fortalte meg akkurat at dere har lært om :point_down:\n\n>${description}\n${moreInfoText}`
      ),
    },
  ];
  if (tags?.length) blocks.push(createTagSection(tags));
  blocks.push({ type: "divider" });
  blocks.push(openModalButton());

  return {
    text: `<@${registrar}> fortalte meg akkurat at dere har lært om ...`,
    blocks,
  };
}

export function thankYouForSharing() {
  return { text: thanksForSharingText() };
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
          if (tags?.length) blocks.push(createTagSection(tags));
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

export async function learnerSummary(
  learnings: Learning[],
  members: Member[],
  sinceDate: Date
) {
  const allLearners = getLearnersFromLearnings(learnings);
  const learningsGroupedByUser = groupLearningsByUser(learnings, allLearners);
  const blocks: Blocks = [
    {
      type: "section",
      text: markdown(
        `*Nå er det på tide å skryte litt av all læringen som skjer i Alv :alv: *\nPå ${formatDistance(
          new Date(),
          sinceDate,
          { locale: nb }
        )} har det blitt registrert *${
          learnings.length
        }* læringsaktiviteter fordelt på *${allLearners.length}* alver. ${
          learnings.length ? "Se hva alle de flinke Alvene lærer seg" : ""
        }`
      ),
    },
    {
      type: "divider",
    },
  ];
  const userSections: Blocks = [];
  for (const userId of Object.values(allLearners)) {
    const member = members.find((m) => m.id === userId);
    userSections.push(
      await createUserSection(member, learningsGroupedByUser[userId])
    );
    userSections.push({ type: "divider" });
  }
  blocks.push(...userSections);
  blocks.push(openModalButton());

  return {
    channel: LEARNING_COLLECTOR_SHARING_CHANNEL_ID,
    text: "Alt det Alvene har lært denne uken",
    blocks,
  };
}

async function createUserSection(member: Member, learnings: Learning[]) {
  const accumulatedReactions = await getReactionsCount(learnings);
  const reactionsText = createReactionsText(accumulatedReactions);
  const userLearnings = learnings
    .map(createLearningNumberedListEntry)
    .join("\n\n");
  return {
    type: "section",
    text: markdown(`*<@${member.id}>*\n\n${userLearnings}\n\n${reactionsText}`),
    accessory: profilePhoto(member),
  };
}

async function getReactionsCount(learnings: Learning[]) {
  let accumulatedReactions: Reaction[] = [];
  for (const { shareMessage } of learnings) {
    if (!shareMessage.channel) continue;
    const reactions = await getReactionsFromMessage(shareMessage);
    accumulatedReactions = reactions.reduce((accumulated, reaction) => {
      const index = accumulated.findIndex((r) => r.name === reaction.name);
      if (index >= 0) {
        accumulated[index].count = +reaction.count;
      } else {
        accumulated.push(reaction);
      }
      return accumulated;
    }, accumulatedReactions);
  }
  return accumulatedReactions;
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

function createLearningNumberedListEntry(learning: Learning, index: number) {
  const { description, locationOfDetails } = learning;
  return `*${index + 1}. ${description}* ${
    locationOfDetails ? `:point_right: ${locationOfDetails}` : ""
  }\n${createTagText(learning.tags)}`;
}

function profilePhoto(member: Member) {
  const image_url = member.profile.image_512;
  return {
    type: "image",
    image_url,
    alt_text: `Profile photo of ${member.name}`,
  };
}

export const TAG_BUTTON_CLICKED = "tag_button_clicked";
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

function createTagSection(tags: string[]) {
  const text = createTagText(tags);
  return {
    type: "section",
    text: markdown(text),
  };
}

function createTagText(tags: string[]) {
  return tags.map((tag) => `*+* _${tag}_`).join("  ");
}
