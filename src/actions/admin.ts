import env from "../environment";
import respondToResponseURL from "../response/respondToResponseURL";
import getMembers from "../slack/getMembers";
import { State } from "./index";

export async function admin(state: State) {
  const { commandBody } = state;

  if (!env.ADMIN_USERS.includes(commandBody.user_id)) {
    respondToResponseURL(commandBody.response_url, {
      text: "Du er ikke en adim bruker!",
    });
    return;
  }

  const adminCommand = commandBody.text.split(" ")[1];
  if (!adminCommand) {
    respondToResponseURL(commandBody.response_url, {
      text: "Admin komandoer krever en komando",
    });
    return;
  }

  switch (adminCommand.toUpperCase()) {
    case "MEMBERS":
      await members(state);
      break;
    default:
      break;
  }
}

async function members({ commandBody }: State) {
  const members = await getMembers();
  respondToResponseURL(commandBody.response_url, {
    text: JSON.stringify(members),
  });
}
