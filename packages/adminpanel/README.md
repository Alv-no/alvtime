# Alvtime-admin-react-pwa

## Setting up the development environment locally

Dependencies:

- [Node.js 14](https://nodejs.org/en/)

Make sure that the backend api is up and running. For the adminpanel to work it needs contact with a backend api service. Have a look in the `packages/api/Readme.md` file for instructions on how to set this up. From this folder run `npm install`, then `npm start` and open the adminpanel in your browser at `localhost:3000`. This page will be redirected to Azure Ad for login. Ask your colleagues for the username and password for the currently used development account. The development server recompiles automatically when the code is changed.

Before submitting your code through a pull request, make sure to run the tests. This is done by running `npm test`.

## Setting up the development container using VsCode Remote containers

Follow these steps to open this project in a development container:

1. If this is your first time using a development container, please follow the [getting started steps](https://aka.ms/vscode-remote/containers/getting-started).

2. In Visual Studio Code, press <kbd>F1</kbd> and select the **Remote-Containers: Open Folder in Container...** command. Select the cloned copy of this folder, wait for the container to start, and try things out!
