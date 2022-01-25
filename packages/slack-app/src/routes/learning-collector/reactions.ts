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
  return [firstEmoji, secondEmoji];
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
