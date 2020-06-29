import { Member } from "../response/createDMChannel";
import { slackWebClient } from "../routes/slack";

export default async function getMembers() {
  const res = ((await slackWebClient.users.list()) as unknown) as {
    members: Member[];
  };
  return res.members.filter(isStartedAlvMember);
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
    "U010SSZAT4N",
    "U011YHQHLGK",
    "U0138NPLV1P",
    "U01533B2BNE",
    "U0154NBUA3W",
    "UNC851SAG",
  ];
  const notStarted = notStartedMembers.includes(member.id);
  return !notStarted;
}
