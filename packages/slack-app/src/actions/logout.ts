import respondToResponseURL from "../response/respondToResponseURL";
import { State } from ".";
import userDB from "../models/user";
import { logoutMessage } from "../messages";

export async function logout(state: State) {
  await userDB.deleteById(state.userData.slackUserID);
  respondToResponseURL(state.commandBody.response_url, logoutMessage());
}
