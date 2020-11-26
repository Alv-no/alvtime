# Alvtime-admin-react-pwa

## Start Development environment

The first time this command is ran it will download and start the local developmment environment. As well as start the development server in the alvtime-admin-react-pwa container. The development server recompiles and refreshes the browser on every detected change to the source code.

```
./run start
```

If you are using windows you can replace ``./run`` with ``npm run``

```
npm run start 
```

- The web app is available on `localhost:3000`
- Swagger documentation is available on `localhost:8000/swagger`
- The api is available on `localhost:8000/api`

To stop and remove the containers run

```
./run down
```

To view the backend logs run.

```
./run logs
```

Download the latest backend api and database images

```
./run pull
```
