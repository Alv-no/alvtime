# Infrastructure

## Dependencies

- [azure cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) version 2.20.0 or later
- [terraform cli](https://learn.hashicorp.com/tutorials/terraform/install-cli) version 0.14.0 or later
- [kubectl](https://v1-18.docs.kubernetes.io/docs/tasks/tools/install-kubectl/) version 1.19.7 or later

# How to plan infrastructure

Login to azure using azure cli `az login`. To plan stage-1 change directory into stage-1 and run `./plan.sh test` or `./plan.sh prod`. Make sure to run the `cleanup.sh` script in `packages/infrastructure` before switching environments. Stage-2 is dependent on outputs from stage-1, so make sure to run the plan script in stage-1 at least once before atempting to run the plan script in stage-2. To plan stage-2 change directory into stage-2 and run `./plan.sh test` or `./plan.sh prod`.
