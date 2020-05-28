import fetch from "node-fetch";

export default function (responsURL: string, message: any) {
  const method = "post";
  const headers = { "Content-type": "application/json" };
  const body = JSON.stringify(message);
  const options = { method, headers, body };
  fetch(responsURL, options);
}
