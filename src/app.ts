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
  auth: {
    user: env.DB_USER,
    password: env.DB_PASSWORD,
  },
  dbName: "alvtime-slack-app",
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
app.use("/something-went-wrong", (req, res) => {
  res.status(500).send(createErrorView());
  req.log.warn("Respond with error view");
});

app.use(errorHandler);

function errorHandler(
  err: { stack: string },
  _req: {},
  res: { redirect: (s: string) => void },
  _next: () => void
) {
  logger.error(err.stack);
  res.redirect("/something-went-wrong");
}

startReminders();

// Starts server
const port = env.PORT || 3000;
app.listen(port, function () {
  logger.info("Alvtime slack app is listening on port " + port);
});
