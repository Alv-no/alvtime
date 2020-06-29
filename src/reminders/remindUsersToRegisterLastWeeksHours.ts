import { Moment } from "moment";
import alvtimeClient, {
  getLastWeekReport,
  UserReports,
} from "../alvtime/alvtimeClient";
import { Task } from "../client";
import config from "../config";
import env from "../environment";
import {
  registerHoursReminderMessage,
  reminderToRegisterHoursAndActivateMessage,
  TokenPayload,
} from "../messages/index";
import userDB, { UserData } from "../models/user";
import configuredMoment from "../moment";
import { createDMChannel, Member } from "../response/createDMChannel";
import { holidays } from "./index";
import getTeamInfo, { TeamInfo } from "../slack/getTeamInfo";
import getAlvMembers from "../slack/getMembers";

export async function remindUsersToRegisterLastWeeksHours() {
  const [users, slackMembers, report, teamInfo, tasks] = await Promise.all([
    userDB.getAll(),
    getAlvMembers(),
    getLastWeekReport(),
    getTeamInfo(),
    alvtimeClient.getTasks(env.REPORT_USER_PERSONAL_ACCESS_TOKEN),
  ]);
  for (const member of slackMembers) {
    await sendReminderToSlacker(users, member, teamInfo, report, tasks);
  }
}

async function sendReminderToSlacker(
  users: UserData[],
  member: Member,
  teamInfo: TeamInfo,
  report: UserReports,
  tasks: Task[]
) {
  const activatedUser = users.find((user) => user.slackUserID === member.id);
  const { channel, postMessage } = await createDMChannel(member);
  if (!activatedUser) {
    const tokenPayload: TokenPayload = {
      slackUserName: member.name,
      slackUserID: member.id,
      slackChannelID: channel.id,
      slackTeamDomain: teamInfo.team.domain,
    };
    const message = reminderToRegisterHoursAndActivateMessage(tokenPayload);
    postMessage(message);
  } else if (shouldRegisterMoreHours(activatedUser, report)) {
    const email = activatedUser.email.toLowerCase();
    const userReport = report[email] ? report[email] : { sum: 0, entries: [] };
    const message = registerHoursReminderMessage(member.id, userReport, tasks);
    postMessage(message);
  }
}

function shouldRegisterMoreHours(activatedUser: UserData, report: UserReports) {
  const sunday = configuredMoment().add(-1, "day");
  const weekHoursGoal = weekGoal(sunday);
  const email = activatedUser.email.toLowerCase();
  return !report[email] || report[email].sum < weekHoursGoal;
}

function weekGoal(mondayOfWeek: Moment): number {
  const week = createWeek(mondayOfWeek);
  let goal = 0;
  for (const date of week) {
    if (!isNonWorkDay(date)) {
      goal = goal + config.HOURS_IN_WORKDAY;
    }
  }
  return goal;
}

function isNonWorkDay(date: Moment): boolean {
  const isHoliday = holidays.isHoliday(date);
  const isSunday = date.day() === 0;
  const isSaturday = date.day() === 6;
  return isHoliday || isSunday || isSaturday;
}

function createWeek(day: Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map((n) => monday.clone().add(n, "day"));
}
