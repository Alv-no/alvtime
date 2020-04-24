# Alvtime-admin-react-pwa

## Start Development environment

The first time this command is ran it will download and start the local developmment environment. As well as start the development server in the alvtime-admin-react-pwa container. The development server recompiles and refreshes the browser on every detected change to the source code.

```
docker-compose up --detach && docker-compose exec alvtime-admin-react-pwa node node_modules/react-scripts/scripts/start.js
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
