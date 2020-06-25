import express from "express";
import mongoose from "mongoose";
import env from "./environment";
import startReminders from "./reminders/index";
import oauth2Router from "./routes/auth/index";
import slackRouter from "./routes/slack";
import createErrorView from "./views/error";

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
    console.log("Database connected");
  })
  .catch((error) => {
    console.error("Database connection error: " + error);
  });

app.use(express.static("public"));
app.use("/slack", slackRouter);
app.use("/oauth2", oauth2Router);
app.use("/something-went-wrong", (_req, res) => {
  res.status(500).send(createErrorView());
});

app.use(errorHandler);

function errorHandler(
  err: { stack: string },
  _req: {},
  res: { redirect: (s: string) => void },
  _next: () => void
) {
  console.error(err.stack);
  res.redirect("/something-went-wrong");
}

startReminders();

// Starts server
const port = env.PORT || 3000;
app.listen(port, function () {
  console.log("Alvtime slack app is listening on port " + port);
});
