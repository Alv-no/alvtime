import express from "express";
import oauth2Router from "./routes/oauth2";
import slackRouter from "./routes/slack";

const app = express();

app.use("/slack", slackRouter);
app.use("/oauth2", oauth2Router);

// Starts server
const port = process.env.PORT || 3000;
app.listen(port, function () {
  console.log("Alvtime slack app is listening on port " + port);
});
