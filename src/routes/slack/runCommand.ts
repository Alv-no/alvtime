import userDB from "../../models/user";
import { CommandBody } from "./slashCommand";
import createCommands from "../../actions/index";
import { remindUsersToRegisterLastWeeksHours } from "../../reminders/remindUsersToRegisterLastWeeksHours";

const { LOGG, TASKS, REG, UKE } = Object.freeze({
  TASKS: "TASKS",
  LOGG: "LOGG",
  REG: "REG",
  UKE: "UKE",
});

export default async function runCommand(commandBody: CommandBody) {
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

    case "TEST":
      remindUsersToRegisterLastWeeksHours();
      break;

    default:
      break;
  }
}
