#!/usr/bin/env bash

set -e

docker push "acralvtime.azurecr.io/alvtime-frontend:$SHORT_HASH"
docker push "acralvtime.azurecr.io/alvtime-admin:$SHORT_HASH"
docker push "acralvtime.azurecr.io/alvtime-web-api:$SHORT_HASH"
docker push "acralvtime.azurecr.io/alvtime-slack-app:$SHORT_HASH"
