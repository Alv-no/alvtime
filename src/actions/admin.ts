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
  const memebersParsed = members.map((member) => ({
    id: member.id,
    name: member.name,
    deleted: member.deleted,
    profile: {
      email: member.profile.email,
    },
    is_bot: member.is_bot,
    is_restricted: member.is_restricted,
    is_ultra_restricted: member.is_ultra_restricted,
    is_stranger: member.is_stranger,
  }));
  respondToResponseURL(commandBody.response_url, {
    text: JSON.stringify(memebersParsed),
  });
}
