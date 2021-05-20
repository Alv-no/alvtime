#!/usr/bin/env bash

set -e

TAG="${SHORT_HASH:-latest}"
docker build . --target dev --tag "acralvtime.azurecr.io/alvtime-frontend:$TAG"
docker run "acralvtime.azurecr.io/alvtime-frontend:$TAG" npm run test
docker run "acralvtime.azurecr.io/alvtime-frontend:$TAG" npm run lint
docker build . --tag "acralvtime.azurecr.io/alvtime-frontend:$TAG"
