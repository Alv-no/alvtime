# Alvtime-slack-app

This app i built using [node slack sdk](https://slack.dev/node-slack-sdk/). Read the [Getting Started with node slack sdk](https://slack.dev/node-slack-sdk/) docs for a in-depth info on how to contribute to this project.

## Scafolding

- `src/app.ts` contains the primary app. It imports the SDK and starts the app/server. It's where you'll add your app's listeners.

## Setting up the development environment locally

### Dependencies

- Node v20.17.0
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

The only way we know how to develop the slack-app is by using the docker-compose setup. The dev environment needs some secrets we have chosen to store in a azure key vault. To get access to this key vault login using `az login`. You can now run `./run slack-app` from the root of the alvtime project. This command will start the necessary services and drop you into a bash shell in the slack app development service. Here you can run `npm run watch` to start the development server. Navigate to the [development slack workspace](dev-qje3117.slack.com/) and log in useng Ahre-Ketil-lillehagen-on-dev-qje3117-slack-com credentials found in our common test user keyvault. In the slack workspace you can run `/alvtime tasks` in anny channel. This will prompt a login flow. Use Ahre-Ketil's azure account to login to azure. The credentials can be found in the same keyvault.
