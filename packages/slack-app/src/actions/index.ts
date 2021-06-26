import { UserData } from "../models/user";
import getAccessToken from "../routes/auth/getAccessToken";
import { CommandBody } from "../routes/slack/slashCommand";
import { admin } from "./admin";
import { logg } from "./logg";
import { register, registerWeek, registerVacation } from "./register";
import { tasks } from "./tasks";

export interface State {
  accessToken: string;
  params: string[];
  commandBody: CommandBody;
  userData: UserData;
}

export default async function createCommands(
  params: string[],
  commandBody: CommandBody,
  userData: UserData
) {
  const accessToken = await getAccessToken(userData);

  const state = {
    accessToken,
    params,
    commandBody,
    userData,
  };

  return {
    logg() {
      logg(state);
    },
    tasks() {
      tasks(state);
    },
    register() {
      register(state);
    },
    registerWeek() {
      registerWeek(state);
    },
    registerVacation() {
      registerVacation(state);
    },
    admin() {
      admin(state);
    },
  };
}

