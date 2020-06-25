import { Moment } from "moment";
import cron from "node-cron";
import alvtimeClient from "../alvtimeClient";
import { ReportTimeEntrie } from "../client/index";
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
import { slackInteractions, slackWebClient } from "../routes/slack";
import createNorwegianHolidays from "../services/holidays";

const holidays = createNorwegianHolidays([configuredMoment().year()]);

// const fridays1630 = "30 14 * * 5"; // UTC time
// const thursdays1900 = "0 17 * * 4"; // UTC time
const monday0900 = "0 7 * * 1";

export default startLastWeeksHoursReminder;
function startLastWeeksHoursReminder() {
  cron.schedule(monday0900, remindUsersToRegisterLastWeeksHours);

  slackInteractions.action(
    { actionId: "open_alvtime_button" },
    async function () {
      return {
        text: "Åpner Alvtime...",
      };
    }
  );
}

export async function remindUsersToRegisterLastWeeksHours() {
  try {
    const [users, slackMembers, report, teamInfo, tasks] = await Promise.all([
      userDB.getAll(),
      getMembers(),
      getLastWeekReport(),
      getTeamInfo(),
      alvtimeClient.getTasks(env.REPORT_USER_PERSONAL_ACCESS_TOKEN),
    ]);

    for (const member of slackMembers) {
      const activatedUser = users.find(
        (user) => user.slackUserID === member.id
      );

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
        const userReport = report[email]
          ? report[email]
          : { sum: 0, entries: [] };
        const message = registerHoursReminderMessage(
          member.id,
          userReport,
          tasks
        );
        postMessage(message);
      }
    }
  } catch (error) {
    console.error(error);
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

export function createWeek(day: Moment) {
  const monday = day.clone().startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map((n) => monday.clone().add(n, "day"));
}

async function getMembers() {
  const res = ((await slackWebClient.users.list()) as unknown) as {
    members: Member[];
  };

  return res.members.filter(isStartedAlvMember);
}

interface TeamInfo {
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

async function getTeamInfo(): Promise<TeamInfo> {
  const teamInfo = ((await slackWebClient.team.info({
    token: env.SLACK_BOT_TOKEN,
  })) as unknown) as TeamInfo;
  return teamInfo;
}

function isStartedAlvMember(member: Member) {
  return isNotABot(member) && hasMemberStarted(member) && isNotExternal(member);
}

function isNotABot(member: Member) {
  return !member.is_bot && member.id !== "USLACKBOT";
}

function isNotExternal(member: Member) {
  const { is_restricted, is_ultra_restricted, is_stranger } = member;
  const isExternal = is_restricted || is_ultra_restricted || is_stranger;
  return !isExternal;
}

function hasMemberStarted(member: Member) {
  const notStartedMembers = [
    "U0154NBUA3W",
    "U011YHQHLGK",
    "UNC851SAG",
    "U0138NPLV1P",
    "U010SSZAT4N",
  ];
  const notStarted = notStartedMembers.includes(member.id);
  return !notStarted;
}

async function getLastWeekReport() {
  const sunday = configuredMoment().startOf("week").add(-1, "day");
  const monday = sunday.clone().startOf("week");
  const lastWeekDateRange = {
    fromDateInclusive: monday.format(config.DATE_FORMAT),
    toDateInclusive: sunday.format(config.DATE_FORMAT),
  };
  const timeEntriesReport = await alvtimeClient.getTimeEntriesReport(
    lastWeekDateRange,
    env.REPORT_USER_PERSONAL_ACCESS_TOKEN
  );

  return createUserReport(timeEntriesReport);
}

export interface UserReports {
  [key: string]: UserReport;
}

export interface UserReport {
  sum: number;
  entries: { taskId: number; date: string; value: number }[];
}

function createUserReport(timeEntriesReport: ReportTimeEntrie[]): UserReports {
  const userReports: UserReports = {};

  for (const entrie of timeEntriesReport) {
    const { userEmail, taskId, date, value } = entrie;
    const email = userEmail.toLowerCase();

    if (!userReports[email]) userReports[email] = { sum: 0, entries: [] };

    userReports[email].entries.push({
      taskId: taskId,
      date: date,
      value: value,
    });

    userReports[email].sum += value;
  }

  return userReports;
}
