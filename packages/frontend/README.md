# alvtime-vue

## Setting up the development environment locally

### Special for windows

Please note that npm uses cmd by default and that doesn't support command substitution, so if you want to leverage that, then you need to update your [.npmrc](.npmrc) to set the script-shell to powershell. (From [cross-env](https://www.npmjs.com/package/cross-env#windows-issues) on npm)

```shell
# set the path to your script shell
script-shell = "C:\\windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe">

#using the CLI
npm config set script-shell = "C:\\windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe"
```

### Dependencies:

- [Node.js 20](https://nodejs.org/en/)

From this folder run `npm install` and `npm start`, then open the frontend in your browser at `localhost:8080`. This starts a development server that recompiles when the code is changed. For the frontend to work it needs contact with a backend api service. Have a look in the [packages/api/Readme.md](../api/Readme.md) file for instructions on how to set this up. The configuration file for the frontend is located in [public/config.json](public/config.json). Edit the `API_HOST` value to make sure it is pointing to the correct backend

Before submitting your code through a pull request, make sure to run the tests and the linter. This is done by running `npm test` and `npm run lint` respectively.

## Setting up the development container using VsCode Remote containers

Follow these steps to open this project in a development container:

1. If this is your first time using a development container, please follow the [getting started steps](https://aka.ms/vscode-remote/containers/getting-started).

2. In Visual Studio Code, press <kbd>F1</kbd> and select the **Remote-Containers: Open Folder in Container...** command. Select the cloned copy of this folder, wait for the container to start, and try things out!
