export function plainText(text: string) {
  return { type: "plain_text", text, emoji: true };
}

export function markdown(text: string) {
  return { type: "mrkdwn", text };
}
