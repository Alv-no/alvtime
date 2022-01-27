import { boltApp } from ".";

export function getReactions() {
  const emojis = [
    "tada",
    "brain",
    "thumbsup",
    "heart_eyes",
    "exploding_head",
    "the_horns",
    "bulb",
    "superhero",
    "mega",
    "clap",
  ];
  let randomIndex = getRandomNumber(emojis.length);
  const firstEmoji = emojis[randomIndex];
  emojis.splice(randomIndex);
  randomIndex = getRandomNumber(emojis.length);
  const secondEmoji = emojis[randomIndex];
  return getRandomNumber(2) === 1 ? [firstEmoji, secondEmoji] : [firstEmoji];
}

export function getRandomNumber(amount: number) {
  return Math.floor(Math.random() * amount);
}

interface MessageRef {
  channel?: string;
  timestamp?: string;
}

export async function getReactionsFromMessage({
  channel,
  timestamp,
}: MessageRef) {
  const {
    message: { reactions },
  } = await boltApp.client.reactions.get({
    channel,
    timestamp,
    full: true,
  });
  return reactions;
}

export function getMoreVoteReactions() {
  const reactions = ["rocket", "star-struck", "handshake", "star2"];
  const voteReactionsTexts = [
    `:${reactions[0]}:  Viktig for Alv`,
    `:${reactions[1]}:  Jeg vil også lære meg dette`,
    `:${reactions[2]}:  Jeg lærer meg også dette`,
    `:${reactions[3]}:  Jeg kan dette og har hatt masse bruk for det. Kan anbefale å jobbe videre med det`,
  ];
  return { reactions, voteReactionsTexts };
}

export function getVoteReactions() {
  const reactions = ["thumbsup"];
  const voteReactionsTexts = [`:${reactions[0]}:  Bra jobba!`];
  return { reactions, voteReactionsTexts };
}
