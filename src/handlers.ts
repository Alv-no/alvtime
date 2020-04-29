import { WebClient } from "@slack/web-api";
import { messageJsonBlock, modalJsonBlock } from "./blocks";

const token = process.env.SLACK_BOT_TOKEN;
const webClient = new WebClient(token);

export async function onAppMention(event: any) {
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
}

export async function onOpenModalButton(payload: any) {
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
}

export async function onCuteAnimalModalSubmit(payload: any) {
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
}
