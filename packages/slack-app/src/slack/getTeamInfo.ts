import env from "../environment";
import { slackWebClient } from "../routes/slack";

export default async function getTeamInfo(): Promise<TeamInfo> {
  const teamInfo = ((await slackWebClient.team.info({
    token: env.SLACK_BOT_TOKEN,
  })) as unknown) as TeamInfo;
  return teamInfo;
}

export interface TeamInfo {
  ok: boolean;
  team: {
    id: string;
    name: string;
    domain: string;
    email_domain: string;
    icon: {
      image_34: string;
      image_44: string;
      image_68: string;
      image_88: string;
      image_102: string;
      image_132: string;
      image_230: string;
      image_default: boolean;
    };
  };
  response_metadata: {
    scopes: string[];
    acceptedScopes: string[];
  };
}

