import {
  admin,
  logg,
  register,
  registerVacation,
  registerWeek,
  tasks,
  logout,
} from "../../actions";
import { logger } from "../../createLogger";
import userDB, { UserData } from "../../models/user";
import { CommandBody } from "./slashCommand";

const { LOGG, TASKS, REG, UKE, ADMIN, FERIE, LOGOUT } = Object.freeze({
  TASKS: "TASKS",
  LOGG: "LOGG",
  REG: "REG",
  UKE: "UKE",
  ADMIN: "ADMIN",
  FERIE: "FERIE",
  LOGOUT: "LOGOUT",
});

export default async function runCommand(
  commandBody: CommandBody,
  userData: UserData
) {
  try {
    const textArray = commandBody.text.split(" ");
    const command = textArray[0].toUpperCase();
    const params = textArray.filter((_t, i) => i !== 0);
    const accessToken = userData.auth.accessToken;
    const state = {
      accessToken,
      params,
      commandBody,
      userData,
    };

    switch (command) {
      case LOGG:
        logg(state);
        break;

      case TASKS:
        tasks(state);
        break;

      case REG:
        register(state);
        break;

      case UKE:
        registerWeek(state);
        break;

      case FERIE:
        registerVacation(state);
        break;

      case ADMIN:
        admin(state);
        break;

      case LOGOUT:
        logout(state);
        break;

      default:
        break;
    }
  } catch (e) {
    logger.error(e);
  }
}
