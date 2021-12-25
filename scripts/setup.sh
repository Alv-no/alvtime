#!/usr/bin/env bash

set -e

SHORT_HASH=$(git rev-parse --short=7 HEAD)
echo SHORT_HASH="$SHORT_HASH"
export SHORT_HASH
ENV=$(echo "$1" | awk '{print tolower($0)}')
echo ENV="$ENV"
export ENV
CONTAINER_REGISTRY=alvkubernetesclustertestacr
echo CONTAINER_REGISTRY="$CONTAINER_REGISTRY"
export CONTAINER_REGISTRY
KEY_VAULT="k8sconfig-$ENV-kv"
echo KEY_VAULT="$KEY_VAULT"
RESOURCE_GROUP_NAME="k8scluster-$ENV-rg"
echo RESOURCE_GROUP_NAME="$RESOURCE_GROUP_NAME"
KUBERNETES_CLUSTER_NAME="k8scluster-$ENV-aks"
echo KUBERNETES_CLUSTER_NAME="$KUBERNETES_CLUSTER_NAME"
SUBSCRIPTION="k8s-$ENV-subscription"
echo SUBSCRIPTION="$SUBSCRIPTION"
if [ "$ENV" = 'review' ]; then
  SUBDOMAIN="$ENVIRONMENT_SLUG"
  echo SUBDOMAIN="$SUBDOMAIN"
  KEY_VAULT="k8sconfig-dev-kv"
  echo KEY_VAULT="$KEY_VAULT"
  RESOURCE_GROUP_NAME="k8scluster-dev-rg"
  echo RESOURCE_GROUP_NAME="$RESOURCE_GROUP_NAME"
  KUBERNETES_CLUSTER_NAME="k8scluster-dev-aks"
  echo KUBERNETES_CLUSTER_NAME="$KUBERNETES_CLUSTER_NAME"
  SUBSCRIPTION="k8s-dev-subscription"
  echo SUBSCRIPTION="$SUBSCRIPTION"
fi
export KEY_VAULT
export RESOURCE_GROUP_NAME
export KUBERNETES_CLUSTER_NAME
export SUBSCRIPTION

function getSecret() {
  az keyvault secret show --vault-name "$KEY_VAULT" --name "$1" | jq '.value' -r
}

echo "Getting secrets from key vault $KEY_VAULT..."
HOSTNAME="$(getSecret alvtime-hostname)"
if [ "$ENV" = 'review' ]; then
  HOSTNAME=""
fi
export HOSTNAME
REPORT_USER_PERSONAL_ACCESS_TOKEN="$(getSecret report-user-personal-access-token)"
export REPORT_USER_PERSONAL_ACCESS_TOKEN
SLACK_ADMIN_USERS="$(getSecret slack-admin-users)"
export SLACK_ADMIN_USERS
SLACK_BOT_TOKEN="$(getSecret slack-bot-token)"
export SLACK_BOT_TOKEN
SLACK_SIGNING_SECRET="$(getSecret slack-signing-secret)"
export SLACK_SIGNING_SECRET
SP_ALVTIME_AUTH_SLACK_APP_SECRET="$(getSecret sp-alvtime-auth-slack-app-secret)"
export SP_ALVTIME_AUTH_SLACK_APP_SECRET
MONGO_DB_ENCRYPTION_KEY="$(getSecret mongo-db-encryption-key)"
export MONGO_DB_ENCRYPTION_KEY
MONGO_DB_CONNECTION_STRING="$(getSecret mongo-db-connection-string)"
export MONGO_DB_CONNECTION_STRING
SQL_CONNECTION_STRING="$(getSecret sql-connection-string)"
export SQL_CONNECTION_STRING
SP_ALVTIME_ADMIN_CLIENT_ID="$(getSecret sp-alvtime-admin-client-id)"
export SP_ALVTIME_ADMIN_CLIENT_ID
SP_ALVTIME_ADMIN_RBAC_SECRET="$(getSecret sp-alvtime-admin-rbac-secret)"
export SP_ALVTIME_ADMIN_RBAC_SECRET
