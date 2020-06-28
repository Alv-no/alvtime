import fetch from "node-fetch";
import { logger } from "../createLogger";

export default function (responsURL: string, message: any) {
  const method = "post";
  const headers = { "Content-type": "application/json" };
  const body = JSON.stringify(message);
  const options = { method, headers, body };
  fetch(responsURL, options);
  logger.info({ responsURL, options });
}
