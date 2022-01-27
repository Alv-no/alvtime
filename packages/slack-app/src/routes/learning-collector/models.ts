import { Learning } from "../../models/learnings";
import { Reaction } from "@slack/web-api/dist/response/ChannelsHistoryResponse";
import { Member } from "@slack/web-api/dist/response/UsersListResponse";
import { Block, KnownBlock } from "@slack/bolt";

export type LearningSummary = Learning & {
  reactions: Reaction[];
  member: Member;
};

export type Blocks = (KnownBlock | Block)[];
