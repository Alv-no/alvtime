export async function acknowledge({
  ack,
  next,
}: {
  ack: () => Promise<void>;
  next: () => void;
}) {
  await ack();
  next();
}
