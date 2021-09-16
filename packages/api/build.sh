#!/usr/bin/env bash

set -e

TAG="${SHORT_HASH:-latest}"
docker build . --tag "$CONTAINER_REGISTRY.azurecr.io/alvtime-web-api:$TAG"
