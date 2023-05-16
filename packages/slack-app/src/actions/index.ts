import { UserData } from "../models/user";
import { CommandBody } from "../routes/slack/slashCommand";
import { admin } from "./admin";
import { logg } from "./logg";
import { register, registerVacation, registerWeek } from "./register";
import { tasks } from "./tasks";

export interface State {
  accessToken: string;
  params: string[];
  commandBody: CommandBody;
  userData: UserData;
}

export { logg, tasks, register, registerWeek, registerVacation, admin };
