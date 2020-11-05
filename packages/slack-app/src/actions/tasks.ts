import alvtimeClient from "../alvtime/alvtimeClient";
import { Task } from "../client/index";
import respondToResponseURL from "../response/respondToResponseURL";
import { State } from "./index";
export async function tasks({ params, commandBody, accessToken }: State) {
  const tasks = await alvtimeClient.getTasks(accessToken);
  respondToResponseURL(commandBody.response_url, {
    text: createTasksMessage(tasks, params.includes("alle")),
  });
}
function createTasksMessage(tasks: Task[], all: boolean) {
  let text = "*ID* - *Task* - *Prosjekt* - *Kunde*\n";
  for (const task of tasks) {
    if (task.favorite || all)
      text =
        text +
        `${task.id} - ${task.name} - ${task.project.name} - ${task.project.customer.name}\n`;
  }
  return text;
}

