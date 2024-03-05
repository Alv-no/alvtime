#!/usr/bin/env bash

set -e

TAG="${SHORT_HASH:-latest}"
docker build . --file Dockerfile --tag "$CONTAINER_REGISTRY.azurecr.io/alvtime-slack-app:$TAG"
