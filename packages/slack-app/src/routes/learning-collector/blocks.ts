export function plainText(text: string): {
  type: "plain_text";
  text: string;
  emoji: boolean;
} {
  return { type: "plain_text", text, emoji: true };
}

export function markdown(text: string): { type: "mrkdwn"; text: string } {
  return { type: "mrkdwn", text };
}
