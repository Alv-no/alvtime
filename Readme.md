# AlvTime

> Welcome to the Alvtime monorepo. Alvtime is the internal timekeeping system for Alv AS. Below you can find a list of the services that make up the system.

## Projects

| Project                             | Type      | Description                  |
| ----------------------------------- | --------- | ---------------------------- |
| [Adminpanel](./packages/adminpanel) | Frontend  | Admin panel for Alvtime      |
| [Frontend](./packages/frontend)     | Frontend  | Enduser frontend for Alvtime |
| [Slack-app](./packages/slack-app)   | Slack-app | Slack app                    |
| [Api](./packages/api)               | Backend   | Backend api                  |
| [Infrastructure](./packages/infrastructure) | Infrastructure | Terraform infrastructure setup |
| [Shell](./packages/shell) | Terminal CLI | Terminal ClI tool for interacting with Alvtime |

## Development / Contribution

The following is adapted from [Release Flow - Azure DevOps | Microsoft Docs](https://docs.microsoft.com/en-us/azure/devops/learn/devops-at-microsoft/release-flow)

### 1. Branch

The first step when a developer wants to fix a bug or implement a feature is to create a new branch off of our main integration branch, master. Thanks to Git's lightweight branching model, we create these short-lived "topic" branches any and every time we want to write some code. Developers are encouraged to commit early and to avoid long-running feature branches by using feature flags.

### 2. Push

When the developer is ready to get their changes integrated and ship their changes to the rest of the team, they push their local branch to a branch on the server, and open a pull request.

### 3. Pull Request

We use Github Pull Requests to control how developers branches are merged into master. Pull Requests ensure that our branch policies are satisfied: first, we build the proposed changes and run a quick test pass. Next, we require that one other members of the team review the code and approve the changes. Code review picks up where the automated tests left off, and are particularly good at spotting architectural problems. Manual code reviews ensure that more engineers on the team have visibility into the changes and that code quality remains high.

### 4. Merge

Once all the build policies are satisfied and reviewers have signed off, then the pull request is completed. This means that the topic branch is merged into the main integration branch, master.
