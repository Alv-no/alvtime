# alvtime-vue

## Setting up the development environment locally

Dependencies:

- [Node.js 20](https://nodejs.org/en/)

From this folder run `npm install` and `npm start`, then open the frontend in your browser at `localhost:8080`. This starts a development server that recompiles when the code is changed. For the frontend to work it needs contact with a backend api service. Have a look in the [packages/api/Readme.md](../api/Readme.md) file for instructions on how to set this up. The configuration file for the frontend is located in [public/config.json](public/config.json). Edit the `API_HOST` value to make sure it is pointing to the correct backend  

Before submitting your code through a pull request, make sure to run the tests and the linter. This is done by running `npm test` and `npm run lint` respectively.

## Setting up the development container using VsCode Remote containers

Follow these steps to open this project in a development container:

1. If this is your first time using a development container, please follow the [getting started steps](https://aka.ms/vscode-remote/containers/getting-started).

2. In Visual Studio Code, press <kbd>F1</kbd> and select the **Remote-Containers: Open Folder in Container...** command. Select the cloned copy of this folder, wait for the container to start, and try things out!
