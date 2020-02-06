# alvtime-vue

## Development

The first time this command is ran it will download and start the local developmment environment. As well as start the development server in the alvtime-vue-dev container. The development server recompiles and refreshes the browser on every detected chang to the source code.

```
docker-compose up --detach && docker-compose exec alvtime-vue-dev npx vue-cli-service serve run serve
```

- The web app is available on `localhost:3000`
- Swagger documentation is available on `localhost:3000/swagger`
- The api is available on `localhost:3000/api`

To view the backend logs run.

```
docker-compose logs -f
```

### Compile and minifie

Up 
```
docker-compose -f docker-compose.localBuild.yaml up --build
```
Down
```
docker-compose -f docker-compose.localBuild.yaml down
```


### Run your unit tests

```
docker-compose exec alvtime-vue-dev npx vue-cli-service test:unit
```

### Lints and fixes files

```
docker-compose exec alvtime-vue-dev npx vue-cli-service lint run lint
```

### Customize configuration

See [Configuration Reference](https://cli.vuejs.org/config/).
