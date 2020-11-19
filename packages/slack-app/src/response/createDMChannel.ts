import { slackWebClient } from "../routes/slack";
import { logger } from "../createLogger";

export interface Member {
  id: string;
  name: string;
  deleted: boolean;
  profile: {
    email: string;
  };
  is_bot: boolean;
  is_restricted: boolean;
  is_ultra_restricted: boolean;
  is_stranger: boolean;
}

export async function createDMChannel(member: Member) {
  const { channel } = ((await slackWebClient.conversations.open({
    users: member.id,
  })) as unknown) as {
    channel: {
      id: string;
    };
  };
  const postMessage = (message: any) => {
    slackWebClient.chat.postMessage({
      ...message,
      channel: channel.id,
    });
    logger.info({msg: "Sent direct message", message});
  };
  return {
    channel,
    postMessage,
  };
}
