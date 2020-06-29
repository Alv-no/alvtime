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
  const members = (await getMembers()) as any[];
  const memebersParsed = members.map((member: any) => {
    const profile = member.profile;
    delete profile.image_24;
    delete profile.image_32;
    delete profile.image_48;
    delete profile.image_72;
    delete profile.image_192;
    delete profile.image_512;

    return {
      ...member,
      profile,
    };
  });

  let batch = [];
  for (let index = 0; index < memebersParsed.length; index++) {
    const member = memebersParsed[index];
    batch.push(member);
    if (batch.length >= 10 || index >= memebersParsed.length - 1) {
      respondToResponseURL(commandBody.response_url, {
        text: JSON.stringify(batch),
      });
      batch = [];
    }
  }
}
