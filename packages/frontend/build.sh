#!/usr/bin/env bash

set -e

TAG="${SHORT_HASH:-latest}"
docker build . --target dev --tag "$CONTAINER_REGISTRY.azurecr.io/alvtime-frontend:$TAG"
docker run "$CONTAINER_REGISTRY.azurecr.io/alvtime-frontend:$TAG" npm run test
docker run "$CONTAINER_REGISTRY.azurecr.io/alvtime-frontend:$TAG" npm run lint
docker build . --tag "$CONTAINER_REGISTRY.azurecr.io/alvtime-frontend:$TAG"
