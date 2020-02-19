# alvtime-vue

## Start Development environment

The first time this command is ran it will download and start the local developmment environment. As well as start the development server in the alvtime-vue-dev container. The development server recompiles and refreshes the browser on every detected change to the source code.

```
docker-compose up --detach && docker-compose exec alvtime-vue-dev npx vue-cli-service serve run serve
```

- The web app is available on `localhost:3000`
- Swagger documentation is available on `localhost:3000/swagger`
- The api is available on `localhost:3000/api`

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

### Run production build locally

This is useful for testing real performance and off-line behaviour. The app installs a service worker on the host machine and uses this to serve cached css, js, an images.

```
docker-compose -f docker-compose.localBuild.yaml up --build
```

- The web app is available on `localhost`
- Swagger documentation is available on `localhost/swagger`
- The api is available on `localhost/api`

To stop and remove the containers run

```
docker-compose -f docker-compose.localBuild.yaml down --volumes
```

To view the backend logs run.

```
docker-compose -f docker-compose.localBuild.yaml logs -f
```

Download the latest backend api and database images

```
docker-compose pull
```

### Run unit tests

Run unit tests

```
docker-compose exec alvtime-vue-dev npx vue-cli-service test:unit
```

Run end to end tests

```
docker-compose exec alvtime-vue-dev npx vue-cli-service test:e2e
```

### Lint and fix files

```
docker-compose exec alvtime-vue-dev npx vue-cli-service lint run lint
```

### Customize configuration

See [Configuration Reference](https://cli.vuejs.org/config/).
