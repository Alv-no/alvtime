#!/usr/bin/env bash

set -e

SHORT_HASH=`git rev-parse --short=7 HEAD`
ENV=$(echo "$1" | awk '{print tolower($0)}')
PROJECT="alvtime"
KEY_VAULT="$PROJECT$ENV"
CERT_EMAIL="hei@alv.no"
INFRASTRUCTURE_LOCATION="westeurope"
SUBSCRIPTION_NAME="$(az account show | jq '.name' -r)"

if [ "$ENV" == 'test' ] && [ "$SUBSCRIPTION_NAME" != 'Dev-Alv' ]; then
  az account set --subscription Dev-Alv
fi

if [ "$ENV" == 'prod' ] && [ "$SUBSCRIPTION_NAME" != 'Production-Alv' ]; then
  az account set --subscription Production-Alv
fi

SUBSCRIPTION_NAME="$(az account show | jq '.name' -r)"
echo "Using $SUBSCRIPTION_NAME subscription"
if [ "$2" != "--no-input" ]; then
  echo "Is $SUBSCRIPTION_NAME the correct subscription? (yes/no)"
  read ANSWER
  if [ "$ANSWER" != 'yes' ]; then
    echo ""
    echo "Use the following command to set preferred subscription:"
    echo ""
    echo "  az account set --subscription <subscription-name>"
    echo ""
    exit 1
  fi
fi

function getSecret() {
  az keyvault secret show --vault-name $KEY_VAULT --name $1 | jq '.value' -r
}

echo "Getting secrets from key vault $KEY_VAULT..."
HOSTNAME="$(getSecret hostname)"
REPORT_USER_PERSONAL_ACCESS_TOKEN="$(getSecret report-user-personal-access-token)"
SLACK_ADMIN_USERS="$(getSecret slack-admin-users)"
SLACK_BOT_TOKEN="$(getSecret slack-bot-token)"
SLACK_SIGNING_SECRET="$(getSecret slack-signing-secret)"
SP_ALVTIME_ADMIN_CLIENT_ID="$(getSecret sp-alvtime-admin-client-id)"
SP_ALVTIME_ADMIN_RBAC_SECRET="$(getSecret sp-alvtime-admin-rbac-secret)"
SP_ALVTIME_AUTH_SLACK_APP_SECRET="$(getSecret sp-alvtime-auth-slack-app-secret)"
SQL_SERVER_ADMINISTRATOR_LOGIN="$(getSecret sql-server-administrator-login)"
SQL_SERVER_ADMINISTRATOR_LOGIN_PASSWORD="$(getSecret sql-server-administrator-login-password)"
MONGO_DB_ENCRYPTION_KEY="$(getSecret mongo-db-encryption-key)"

if [ "$ENV" == 'test' ]; then
  SP_ALVTIME_ADMIN_PROD_OBJECT_ID="$(getSecret sp-alvtime-admin-prod-object-id)"
fi
