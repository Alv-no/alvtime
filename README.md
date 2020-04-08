Alvtime-slack-app
=================

This app i built using [Bolt](https://slack.dev/bolt). Read the [Getting Started with Bolt](https://api.slack.com/start/building/bolt) guide for a in-depth tutorial on how to contribute to this project. More info is found in the [Bolt documentation](https://slack.dev/bolt).

Scafolding
----------

- `src/app.ts` contains the primary Bolt app. It imports the Bolt package (`@slack/bolt`) and starts the Bolt app's server. It's where you'll add your app's listeners.
- `.env` is where you'll put your Slack app's authorization token and signing secret.

## Start Development environment

The first time this command is ran it will download and start the local developmment environment. As well as start the development server in the alvtime-slack-app container. The development server recompiles and refreshes the browser on every detected change to the source code.

The command also starts a container running [ngrok](https://ngrok.com), which exposes the Alvtime-slack-app on a public url. This url is useful to connect the slack app configured on at [https://api.slack.com/apps](https://api.slack.com/apps) and test ti in a development slack workspace. The public url is show in the ngrok inspector.

```
docker-compose up
```

- The ngrok inspector is available on `localhost:4040`

To stop and remove the containers run

```
docker-compose down --volumes
```

To view the backend logs run.

```
docker-compose logs -f
```

Download the latest backend api and database images

```
docker-compose pull
```
