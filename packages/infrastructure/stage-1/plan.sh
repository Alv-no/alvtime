#!/usr/bin/env bash

set -e

[ -z "$ENV" ] && \
  source ../../../scripts/setup.sh

terraform init \
  -input=false \
  -backend-config="../$ENV-backend.tfvars" \
  -upgrade \

terraform validate

terraform plan \
  -input=false \
  -var location="$INFRASTRUCTURE_LOCATION" \
  -var name="$PROJECT" \
  -var env="$ENV" \
  -var sp_alvtime_admin_client_id="$SP_ALVTIME_ADMIN_CLIENT_ID" \
  -var sp_alvtime_admin_rbac_secret="$SP_ALVTIME_ADMIN_RBAC_SECRET" \
  -var sql_server_administrator_login="$SQL_SERVER_ADMINISTRATOR_LOGIN" \
  -var sql_server_administrator_login_password="$SQL_SERVER_ADMINISTRATOR_LOGIN_PASSWORD" \
  -var hostname="$HOSTNAME" \
  -var sp_alvtime_admin_prod_object_id="$SP_ALVTIME_ADMIN_PROD_OBJECT_ID" \
  -out=plan \
