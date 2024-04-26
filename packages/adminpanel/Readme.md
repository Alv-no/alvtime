# Alvtime adminpanel blazor app

## Setting up the development environment locally

Dependencies:

- [.NET](https://dotnet.microsoft.com/en-us/download)

Make sure that the backend api is up and running. For the adminpanel to work it needs contact with a backend api service. Have a look in the `packages/api/Readme.md` file for instructions on how to set this up. For running both backend with database together with adminpanel you can run `docker-compose up -d --build adminpanel` from the root alvtime-folder. The adminpanel will run on `localhost:3000`.

For local development you may want to run the app outside of Docker to enable hot reload. In this case, start the backend services using Docker as described in the `Readme.md` for the api, then start the adminpanel either from your preferred IDE or using the `dotnet watch` CLI command. 