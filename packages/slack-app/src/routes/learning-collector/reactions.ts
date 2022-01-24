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
