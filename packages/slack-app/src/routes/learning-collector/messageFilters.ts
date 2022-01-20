export async function isIm({ payload, next }: any) {
  if (payload.channel_type === "im") next();
}
