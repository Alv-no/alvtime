import { Moment } from "moment";
import cron from "node-cron";
import alvtimeClient from "../alvtimeClient";
import { ReportTimeEntrie } from "../client/index";
import config from "../config";
import env from "../environment";
import {
  createRegisterHoursReminder,
  createReminderToRegisterHoursAndActivate,
  TokenPayload,
} from "../messages/index";
import userDB from "../models/user";
import configuredMoment from "../moment";
import { slackInteractions, slackWebClient } from "../routes/slack";
import createNorwegianHolidays from "../services/holidays";

const holidays = createNorwegianHolidays([configuredMoment().year()]);

interface Member {
  id: string;
  name: string;
  deleted: boolean;
  profile: {
    email: string;
  };
  is_bot: boolean;
  is_restricted: boolean;
  is_ultra_restricted: boolean;
  is_stranger: boolean;
}

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

    const sunday = configuredMoment().add(-1, "day");
    const weekHoursGoal = weekGoal(sunday);

    for await (const member of slackMembers) {
      const activatedUser = users.find(
        (user) => user.slackUserID === member.id
      );

      if (!activatedUser) {
        const {
          channel: { id },
        } = ((await slackWebClient.conversations.open({
          users: member.id,
        })) as unknown) as { channel: { id: string } };

        const tokenPayload: TokenPayload = {
          slackUserName: member.name,
          slackUserID: member.id,
          slackChannelID: id,
          slackTeamDomain: teamInfo.team.domain,
        };

        slackWebClient.chat.postMessage({
          ...createReminderToRegisterHoursAndActivate(tokenPayload),
          channel: id,
        });
      } else if (
        !report[activatedUser.email.toLowerCase()] ||
        report[activatedUser.email.toLowerCase()].sum < weekHoursGoal
      ) {
        const {
          channel: { id },
        } = ((await slackWebClient.conversations.open({
          users: member.id,
        })) as unknown) as { channel: { id: string } };

        const email = activatedUser.email.toLowerCase();
        const userReport = report[email]
          ? report[email]
          : { sum: 0, entries: [] };
        slackWebClient.chat.postMessage({
          ...createRegisterHoursReminder(member.id, userReport, tasks),
          channel: id,
        });
      }
    }
  } catch (error) {
    console.error(error);
  }
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
