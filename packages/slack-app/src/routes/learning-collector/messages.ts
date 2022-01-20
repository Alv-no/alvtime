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

export function bostAboutLearning(
  userId: string,
  description: string,
  locationOfDetails: string
) {
  return {
    channel: FAG_CHANNEL_ID,
    text: `<@${userId}> bygger kunnskap som bare det :tada:\n\n>${description}\nFinn mere info :point_down:\n${locationOfDetails}`,
    blocks: [
      {
        type: "section",
        text: markdown(
          `<@${userId}> bygger kunnskap som bare det :tada:\n\n>${description}\nFinn mere info :point_down:\n${locationOfDetails}`
        ),
      },
      openModalButton(),
    ],
  };
}

export const IS_LEARNING_BUTTON_CLICKED = "is_learning_button_clicked";
function openModalButton() {
  return {
    type: "actions",
    elements: [
      {
        type: "button",
        text: plainText("Fortell hva du lærer!"),
        action_id: IS_LEARNING_BUTTON_CLICKED,
      },
    ],
  };
}

export const TAG_BUTTON_CLICKED = "tag_button_clicked"
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
            return [
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
              {
                type: "actions",
                elements: tags.map((tag) => ({
                  type: "button",
                  text: plainText(tag),
                  value: tag,
                  action_id: TAG_BUTTON_CLICKED,
                  url: `https://www.google.com/search?q=${tag}`,
                })),
              },
            ];
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
