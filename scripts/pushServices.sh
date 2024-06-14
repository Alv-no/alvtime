#!/usr/bin/env bash

set -e

docker push "$CONTAINER_REGISTRY.azurecr.io/alvtime-frontend:$SHORT_HASH"
docker push "$CONTAINER_REGISTRY.azurecr.io/alvtime-web-api:$SHORT_HASH"
docker push "$CONTAINER_REGISTRY.azurecr.io/alvtime-slack-app:$SHORT_HASH"
