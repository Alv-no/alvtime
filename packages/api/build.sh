#!/usr/bin/env bash

set -e

TAG="${SHORT_HASH:-latest}"
docker build . --tag "acralvtime.azurecr.io/alvtime-web-api:$TAG"
