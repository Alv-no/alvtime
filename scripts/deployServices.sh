#!/usr/bin/env bash

set -e

cd packages/charts

# Retrieve the access credentials for your cluster and automatically configure kubectl
az aks get-credentials \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --name "$KUBERNETES_CLUSTER_NAME" \

helm upgrade api service \
  --install \
  --values "service/data/api/$ENV-env.yaml" \
  --set image.tag="$SHORT_HASH" \
  --set secrets.ConnectionStrings__AlvTime_db="${SQL_CONNECTION_STRING/,/\\,}" \

helm upgrade slack-api service \
  --install \
  --values "service/data/slack-api/$ENV-env.yaml" \
  --set image.tag="$SHORT_HASH" \
  --set secrets.ADMIN_USERS="$SLACK_ADMIN_USERS" \
  --set secrets.AZURE_AD_CLIENT_SECTRET="$SP_ALVTIME_AUTH_SLACK_APP_SECRET" \
  --set secrets.DB_CONNECTION_STRING="$MONGO_DB_CONNECTION_STRING" \
  --set secrets.DB_PASSWORD="$MONGO_DB_PRIMARY_KEY" \
  --set secrets.DB_ENCRYPTION_KEY="$MONGO_DB_ENCRYPTION_KEY" \
  --set secrets.REPORT_USER_PERSONAL_ACCESS_TOKEN="$REPORT_USER_PERSONAL_ACCESS_TOKEN" \
  --set secrets.SLACK_BOT_TOKEN="$SLACK_BOT_TOKEN" \
  --set secrets.SLACK_SIGNING_SECRET="$SLACK_SIGNING_SECRET" \

helm upgrade frontend service \
  --install \
  --set image.tag="$SHORT_HASH" \
  --values "service/data/frontend/$ENV-env.yaml" \

helm upgrade admin service \
  --install \
  --set image.tag="$SHORT_HASH" \
  --values "service/data/admin/$ENV-env.yaml" \

sleep 20
curl "https://api.$HOSTNAME/api/ping"
curl "https://slack-app.$HOSTNAME"
curl "https://admin.$HOSTNAME"
curl "https://$HOSTNAME"

cd ../..
