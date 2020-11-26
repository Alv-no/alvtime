# alvtime-vue

## Setting up the development container using VsCode Remote containers

Follow these steps to open this project in a development container:

1. If this is your first time using a development container, please follow the [getting started steps](https://aka.ms/vscode-remote/containers/getting-started).

2. In Visual Studio Code, press <kbd>F1</kbd> and select the **Remote-Containers: Open Folder in Container...** command. Select the cloned copy of this folder, wait for the container to start, and try things out!

## Setting up the development container using docker-compose

The first time this command is ran it will download and start the local developmment environment.

```
docker-compose -f .devcontainer/docker-compose.yaml up -d
```

Start development server by running the following. The development server recompiles and refreshes the browser on every detected change to the source code.

```
docker-compose -f .devcontainer/docker-compose.yaml exec alvtime-vue-dev npx vue-cli-service serve
```

- The web app is available on `localhost:3000`
- Swagger documentation is available on `localhost:3000/swagger`
- The api is available on `localhost:3000/api`

To stop the containers run.

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

### Run unit tests

Run unit tests

```
docker-compose -f .devcontainer/docker-compose.yaml exec alvtime-vue-dev npx vue-cli-service test:unit
```

Run end to end tests

```
docker-compose -f .devcontainer/docker-compose.yaml exec alvtime-vue-dev npx vue-cli-service test:e2e
```

### Lint and fix files

```
docker-compose -f .devcontainer/docker-compose.yaml exec alvtime-vue-dev npx vue-cli-service lint run lint
```

## Run production build locally

This is useful for testing real performance and off-line behaviour. The app installs a service worker on the host machine and uses this to serve cached css, js, and images.

```
docker-compose up --build
```

- The web app is available on `localhost`
- Swagger documentation is available on `localhost/swagger`
- The api is available on `localhost/api`

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

## Customize configuration

See [Configuration Reference](https://cli.vuejs.org/config/).
