import { Block, KnownBlock } from "@slack/bolt";
import { Member } from "@slack/web-api/dist/response/UsersListResponse";
import { FAG_CHANNEL_ID } from ".";
import { markdown, plainText } from "./blocks";
import { LearningSummary } from "./models";

type Blocks = (KnownBlock | Block)[];

export function whatUserIsLearningQuestion() {
  return {
    blocks: [
      {
        type: "section",
        text: markdown(
          "Trykk på knapper og fortell meg hva du lærer deg for tiden :wave:"
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

export function bostAboutLearning(state: Learning) {
  const { description, locationOfDetails, learners, tags } = state;
  const usersText = createMultipleUsersText(learners);
  const moreInfoText = locationOfDetails
    ? `\nFinn mere info :point_down:\n${locationOfDetails}`
    : "";
  const text =
    `${usersText} bygger kunnskap som bare det :tada:\n\n>${description}` +
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
    channel: FAG_CHANNEL_ID,
    text: `${usersText} bygger kunnskap som bare det :tada:\n\n>${description}\nFinn mere info :point_down:\n${locationOfDetails}`,
    blocks,
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

export const TAG_BUTTON_CLICKED = "tag_button_clicked";
export function weekSummary(learnings: LearningSummary[]) {
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
    ...learnings
      .map(
        ({
          slackUserID,
          description,
          locationOfDetails,
          reactions,
          member,
          tags,
        }) => {
          const reactionsText = reactions
            .map(({ name, count }) => `:${name}: ${count}`)
            .join("  ");
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
    channel: FAG_CHANNEL_ID,
    text: "backup",
    blocks,
  };
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
