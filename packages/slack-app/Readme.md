# Alvtime-slack-app

This app i built using [node slack sdk](https://slack.dev/node-slack-sdk/). Read the [Getting Started with node slack sdk](https://slack.dev/node-slack-sdk/) docs for a in-depth info on how to contribute to this project.

## Scafolding

- `src/app.ts` contains the primary app. It imports the SDK and starts the app/server. It's where you'll add your app's listeners.
- `.env` is where you'll put your Slack app's authorization token and signing secret.

## Setting up the development container using VsCode Remote containers

Follow these steps to open this project in a development container:

1. If this is your first time using a development container, please follow the [getting started steps](https://aka.ms/vscode-remote/containers/getting-started).

2. In Visual Studio Code, press <kbd>F1</kbd> and select the **Remote-Containers: Open Folder in Container...** command. Select the cloned copy of this folder, wait for the container to start, and try things out!

## Start Development environment using docker-compose

The first time this command is ran it will download and start the local developmment environment.

The command also starts a container running [ngrok](https://ngrok.com), which is used to expose the Alvtime-slack-app on a public url. This url is useful to connect the slack app configured on at [https://api.slack.com/apps](https://api.slack.com/apps) and test it in a development slack workspace. The public url is shown in the ngrok inspector.

Start the environment:

```
docker-compose -f .devcontainer/docker-compose.yaml up -d
```

Start the verification server

```
docker-compose -f .devcontainer/docker-compose.yaml exec alvtime-slack-app npx slack-verify --secret $(grep SLACK_SIGNING_SECRET .env | cut -d '=' -f2)
```

- The ngrok inspector is available on `localhost:4040`

Copy the url in the ngrok inspector and use it as the new request URL under Event Subscriptions on the api.slack.com app setup page to trigger the verification. Stop the verification server and run the following to start the app in watch mode

```
docker-compose -f .devcontainer/docker-compose.yaml exec alvtime-slack-app npm run watch
```

To stop and remove the containers run

```
docker-compose -f .devcontainer/docker-compose.yaml down --volumes
```

To view the backend logs run.

```
docker-compose -f .devcontainer/docker-compose.yaml logs -f
```

Download the latest backend api and database images

```
docker-compose -f .devcontainer/docker-compose.yaml pull
```
