import express from "express";
import mongoose from "mongoose";
import env from "./environment";
import startReminders from "./reminders/index";
import oauth2Router from "./routes/auth/index";
import slackRouter from "./routes/slack";
import createErrorView from "./views/error";
import { logger, loggerMiddleware } from "./createLogger";

const app = express();

const dbOptions = {
  useNewUrlParser: true,
  useUnifiedTopology: true,
  useFindAndModify: false,
  useCreateIndex: true,
  dbName: "slack-app",
};

mongoose
  .connect(env.DB_CONNECTION_STRING, dbOptions)
  .then(() => {
    logger.info("Database connected");
  })
  .catch((error) => {
    logger.error("Database connection error: " + error);
  });

app.use(loggerMiddleware);
app.use(express.static("public"));
app.use("/slack", slackRouter);
app.use("/oauth2", oauth2Router);
app.use("/something-went-wrong", (_req, res) => {
  res.send(createErrorView());
});

app.use(errorHandler);

function errorHandler(
  err: { stack: string },
  _req: { log: { error: (s: string) => void } },
  res: { status: (n: number) => { send: (s: string) => void } },
  _next: () => void
) {
  logger.error(err);
  res.status(500).send(createErrorView());
}

startReminders();

// Starts server
const port = env.PORT || 3000;
app.listen(port, function () {
  logger.info("Alvtime slack app is listening on port " + port);
});
