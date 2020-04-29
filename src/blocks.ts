export const messageJsonBlock = {
  blocks: [
    {
      type: "section",
      text: {
        type: "mrkdwn",
        text: "Hello, thanks for calling me. Would you like to launch a modal?",
      },
      accessory: {
        type: "button",
        action_id: "open_modal_button", // We need to add this
        text: {
          type: "plain_text",
          text: "Launch",
          emoji: true,
        },
        value: "launch_button_click",
      },
    },
  ],
};

export const modalJsonBlock = {
  type: "modal",
  callback_id: "cute_animal_modal_submit", // We need to add this
  title: {
    type: "plain_text",
    text: "My App",
    emoji: true,
  },
  submit: {
    type: "plain_text",
    text: "Done",
    emoji: true,
  },
  close: {
    type: "plain_text",
    text: "Cancel",
    emoji: true,
  },
  blocks: [
    {
      type: "section",
      text: {
        type: "mrkdwn",
        text: "Thanks for openeing this modal!",
      },
    },
    {
      type: "input",
      block_id: "cute_animal_selection_block", // put this here to identify the selection block
      element: {
        type: "static_select",
        action_id: "cute_animal_selection_element", // put this here to identify the selection element
        placeholder: {
          type: "plain_text",
          text: "Select a cute animal",
          emoji: true,
        },
        options: [
          {
            text: {
              type: "plain_text",
              text: "Puppy",
              emoji: true,
            },
            value: "puppy",
          },
          {
            text: {
              type: "plain_text",
              text: "Kitten",
              emoji: true,
            },
            value: "kitten",
          },
          {
            text: {
              type: "plain_text",
              text: "Bunny",
              emoji: true,
            },
            value: "bunny",
          },
        ],
      },
      label: {
        type: "plain_text",
        text: "Choose a cute pet:",
        emoji: true,
      },
    },
    {
      type: "input",
      block_id: "cute_animal_name_block", // put this here to identify the input.
      element: {
        type: "plain_text_input",
        action_id: "cute_animal_name_element", // put this here to identify the selection element
      },
      label: {
        type: "plain_text",
        text: "Give it a cute name:",
        emoji: true,
      },
    },
  ],
};
