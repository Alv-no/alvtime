import createCommands from "../../actions/index";
import userDB from "../../models/user";
import { logger } from "../../createLogger";
import { CommandBody } from "./slashCommand";

const { LOGG, TASKS, REG, UKE, ADMIN, FERIE } = Object.freeze({
  TASKS: "TASKS",
  LOGG: "LOGG",
  REG: "REG",
  UKE: "UKE",
  ADMIN: "ADMIN",
  FERIE: "FERIE",
});

export default async function runCommand(commandBody: CommandBody) {
  try {
    const textArray = commandBody.text.split(" ");
    const command = textArray[0].toUpperCase();
    const params = textArray.filter((_t, i) => i !== 0);
    const userData = await userDB.findById(commandBody.user_id);
    const commands = await createCommands(params, commandBody, userData);

    switch (command) {
      case LOGG:
        commands.logg();
        break;

      case TASKS:
        commands.tasks();
        break;

      case REG:
        commands.register();
        break;

      case UKE:
        commands.registerWeek();
        break;

      case FERIE:
        commands.registerVacation();
        break;

      case ADMIN:
        commands.admin();
        break;

      default:
        break;
    }
  } catch (e) {
    logger.error("error", e);
  }
}
