import { FAG_CHANNEL_ID } from ".";
import { markdown, plainText } from "./blocks";
import { LearningSummary } from "./models";

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

export function bostAboutLearning(state: {
  description: string;
  locationOfDetails: string;
  learners: string[];
  tags: string[];
}) {
  const { description, locationOfDetails, learners, tags } = state;
  const usersText = createMultipleUsersText(learners);
  const moreInfoText = locationOfDetails
    ? `\nFinn mere info :point_down:\n${locationOfDetails}`
    : "";
  const text =
    `${usersText} bygger kunnskap som bare det :tada:\n\n>${description}` +
    moreInfoText;
  const blocks: any = [
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
function openModalButton() {
  return {
    type: "actions",
    elements: [
      {
        type: "button",
        text: plainText("Fortell hva du lærer!"),
        style: "primary",
        action_id: IS_LEARNING_BUTTON_CLICKED,
      },
    ],
  };
}

export const TAG_BUTTON_CLICKED = "tag_button_clicked";
export function weekSummary(learnings: LearningSummary[]) {
  return {
    channel: FAG_CHANNEL_ID,
    text: "backup",
    blocks: [
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
            const image_url = member.profile.image_512;
            const blocks: any = [
              {
                type: "section",
                text: markdown(
                  `*<@${slackUserID}>*\n${reactionsText}\n${description}\nMore info: ${locationOfDetails}`
                ),
                accessory: {
                  type: "image",
                  image_url,
                  alt_text: `Profile photo of ${member.name}`,
                },
              },
            ];
            if (tags?.length) blocks.push(createTagButtons(tags));
            return blocks;
          }
        )
        .flat(),
      {
        type: "divider",
      },
      openModalButton(),
    ],
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
