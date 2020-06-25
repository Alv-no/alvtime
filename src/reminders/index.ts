import cron from "node-cron";
import configuredMoment from "../moment";
import createNorwegianHolidays from "../services/holidays";
import { remindUsersToRegisterLastWeeksHours } from "./remindUsersToRegisterLastWeeksHours";

export const holidays = createNorwegianHolidays([configuredMoment().year()]);

// const fridays1630 = "30 14 * * 5"; // UTC time
// const thursdays1900 = "0 17 * * 4"; // UTC time
const monday0900 = "0 7 * * 1";

export default startReminders;

function startReminders() {
  cron.schedule(monday0900, () => {
    try {
      remindUsersToRegisterLastWeeksHours();
    } catch (error) {
      console.error(error);
    }
  });
}

