#!/usr/bin/env bash

set -e

export HELM_EXPERIMENTAL_OCI=1

echo "$SP_ALVTIME_ADMIN_RBAC_SECRET" | helm registry login "$CONTAINER_REGISTRY.azurecr.io" \
  --username "$SP_ALVTIME_ADMIN_CLIENT_ID" \
  --password-stdin

helm pull "oci://$CONTAINER_REGISTRY.azurecr.io/helm/chart" --version v1 --untar

# Retrieve the access credentials for your cluster and automatically configure kubectl
az aks get-credentials \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --subscription "$SUBSCRIPTION" \
  --name "$KUBERNETES_CLUSTER_NAME"

kubelogin convert-kubeconfig -l azurecli

\cp "packages/api/k8s_environments/$ENV-env.yaml" chart/env.yaml
helm upgrade api ./chart \
  --install \
  --values "chart/env.yaml" \
  --set image.repository="$CONTAINER_REGISTRY.azurecr.io/alvtime-web-api" \
  --set image.tag="$SHORT_HASH" \
  --set secrets.ConnectionStrings__AlvTime_db="${SQL_CONNECTION_STRING/,/\\,}" \
  --namespace "alvtime"

\cp "packages/slack-app/k8s_environments/$ENV-env.yaml" chart/env.yaml
helm upgrade slack-api ./chart \
  --install \
  --values "chart/env.yaml" \
  --set image.tag="$SHORT_HASH" \
  --set image.repository="$CONTAINER_REGISTRY.azurecr.io/alvtime-slack-app" \
  --set secrets.ADMIN_USERS="$SLACK_ADMIN_USERS" \
  --set secrets.AZURE_AD_CLIENT_SECTRET="$SP_ALVTIME_AUTH_SLACK_APP_SECRET" \
  --set secrets.DB_CONNECTION_STRING="$MONGO_DB_CONNECTION_STRING" \
  --set secrets.DB_ENCRYPTION_KEY="$MONGO_DB_ENCRYPTION_KEY" \
  --set secrets.REPORT_USER_PERSONAL_ACCESS_TOKEN="$REPORT_USER_PERSONAL_ACCESS_TOKEN" \
  --set secrets.SLACK_BOT_TOKEN="$SLACK_BOT_TOKEN" \
  --set secrets.SLACK_SIGNING_SECRET="$SLACK_SIGNING_SECRET" \
  --set secrets.LEARNING_COLLECTOR_SLACK_BOT_SIGNING_SECRET="$LEARNING_COLLECTOR_SLACK_BOT_SIGNING_SECRET" \
  --set secrets.LEARNING_COLLECTOR_SLACK_BOT_TOKEN="$LEARNING_COLLECTOR_SLACK_BOT_TOKEN" \
  --set secrets.CVPARTNER_API_TOKEN="$CVPARTNER_API_TOKEN" \
  --namespace "alvtime"

\cp "packages/frontend/k8s_environments/$ENV-env.yaml" chart/env.yaml
\cp "packages/frontend/k8s_environments/$ENV-config.json" chart/config.json
helm upgrade frontend ./chart \
  --install \
  --set image.tag="$SHORT_HASH" \
  --set image.repository="$CONTAINER_REGISTRY.azurecr.io/alvtime-frontend" \
  --set configFilePath="config.json" \
  --values "chart/env.yaml" \
  --namespace "alvtime"

\cp "packages/adminpanel/k8s_environments/$ENV-env.yaml" chart/env.yaml
\cp "packages/adminpanel/k8s_environments/$ENV-config.json" chart/config.json
helm upgrade admin ./chart \
  --install \
  --set image.tag="$SHORT_HASH" \
  --set image.repository="$CONTAINER_REGISTRY.azurecr.io/alvtime-admin" \
  --set configFilePath="config.json" \
  --values "chart/env.yaml" \
  --namespace "alvtime"

sleep 20
curl "https://api.$HOSTNAME/api/ping"
curl "https://slack-app.$HOSTNAME"
curl "https://admin.$HOSTNAME"
curl "https://$HOSTNAME"
