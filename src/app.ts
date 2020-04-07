// Require the Bolt package (github.com/slackapi/bolt)
import { App } from "@slack/bolt";
import dotenv from "dotenv";


if (process.env.NODE_ENV !== "production") {
  dotenv.config({ path: ".env" });
}

const app: App = new App({
  token: process.env.SLACK_BOT_TOKEN,
  signingSecret: process.env.SLACK_SIGNING_SECRET
});


// All the room in the world for your code


(async () => {
  // Start your app
  await app.start(process.env.PORT || 3000);

  console.log("⚡️ Bolt app is running!");
})();
