import env from "../environment";
import respondToResponseURL from "../response/respondToResponseURL";
import { getMembers } from "../slack/getMembers";
import { State } from "./index";
import { remindUsersToRegisterLastWeeksHours } from "../reminders/remindUsersToRegisterLastWeeksHours";

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
    case "09_MONDAY_REMINDER":
      remindUsersToRegisterLastWeeksHours();
      break;
    default:
      break;
  }
}

async function members({ commandBody }: State) {
  const members = (await getMembers()) as any[];
  const memebersParsed = members.map((member: any) => {
    const profile = removeImageRefs(member.profile);

    return {
      ...member,
      profile,
    };
  });

  useBatching(memebersParsed, (batch: any[]) => {
    respondToResponseURL(commandBody.response_url, {
      text: JSON.stringify(batch),
    });
  });
}

// calls callback function with batches of the input array
function useBatching(data: any[], cb: (batch: any[]) => void, batchSize = 10) {
  let batch = [];
  for (let index = 0; index < data.length; index++) {
    batch.push(data[index]);

    if (batch.length >= batchSize || index >= data.length - 1) {
      cb(batch);
      batch = [];
    }
  }
}

function removeImageRefs(profile: any) {
  for (const key of Object.keys(profile)) {
    if (key.includes("image")) {
      delete profile[key];
    }
  }
  return profile;
}
