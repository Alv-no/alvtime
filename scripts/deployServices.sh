#!/usr/bin/env bash

set -e

cd packages/infrastructure/stage-1

MONGO_DB_CONNECTION_STRING="$(terraform output -json mongo_db_connection_strings | jq '.[0]' -r)"
MONGO_DB_PRIMARY_KEY="$(terraform output -raw mongo_db_primary_key)"
SQL_DATABASE_NAME="$(terraform output -raw sql_database_name)"
SQL_SERVER_NAME="$(terraform output -raw sql_server_name)"
SQL_CONNECTION_STRING="Server=tcp:$SQL_SERVER_NAME.database.windows.net\,1433;Initial Catalog=$SQL_DATABASE_NAME;Persist Security Info=False;User ID=$SQL_SERVER_ADMINISTRATOR_LOGIN;Password=$SQL_SERVER_ADMINISTRATOR_LOGIN_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

cd ../../charts

helm upgrade api service \
  --install \
  --values "service/data/api/$ENV-env.yaml" \
  --set image.tag="$SHORT_HASH" \
  --set secrets.ConnectionStrings__AlvTime_db="$SQL_CONNECTION_STRING" \

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
