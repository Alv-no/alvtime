#!/usr/bin/env bash

set -e

if [ ! "$UTILS_LOADED" ]; then
  echo 'Loading utils'
  UTILS_LOADED=true

  if [ ! "$CI" ]; then
    echo 'Loading AZ utils'

    function get_secret() {
      az keyvault secret show --vault-name "$KEY_VAULT" --name "$1" | jq '.value' -r
    }

    function hash_exists_for() {
      az acr repository show-tags \
        --name aizeio \
        --repository "vcp/web-client" \
        --query "[?@ == \`$SHORT_HASH\`] \
      | [0]"
    }
  fi

  if [ "$CI" ]; then
    echo 'Loading CI utils'

    function get_token() {
      curl "https://login.microsoftonline.com/$AZURE_TENANT_ID/oauth2/v2.0/token" \
        --request POST \
        --silent \
        --location \
        --header 'Content-Type: application/x-www-form-urlencoded' \
        --data-urlencode 'grant_type=client_credentials' \
        --data-urlencode 'scope=https://vault.azure.net/.default' \
        --data-urlencode "client_id=$SP_K8S_VCP_ADMIN_ID" \
        --data-urlencode "client_secret=$SP_K8S_VCP_ADMIN_PASSWORD" \
        | jq -r '.access_token'
    }

    echo 'Fetching azure ad token'
    TOKEN="$(get_token)"
    export TOKEN

    function get_secret() {
      curl "https://$KEY_VAULT.vault.azure.net/secrets/$1?api-version=7.2" \
        --request GET \
        --silent \
        --location \
        --header "Authorization: Bearer $TOKEN" \
        | jq -r '.value'
    }

    # https://github.com/Azure/acr/blob/main/docs/AAD-OAuth.md#catalog-listing-using-spadmin-with-basic-auth
    function hash_exists_for() {
      SP_AIZEIO_PASSWORD="$(get_secret sp-aizeio-password)"

      CREDENTIALS=$(echo -n "$SP_AIZEIO_ID:$SP_AIZEIO_PASSWORD" | base64 -w 0)

      curl "https://$CONTAINER_REGISTRY_HOST/acr/v1/$1/_tags?orderby=timedesc&n=10000" \
        --silent \
        --location \
        --header "Authorization: Basic $CREDENTIALS" \
        | jq -r '.tags[].name' \
        | grep "$SHORT_HASH"
    }

  fi

  function acr_login() {
    az acr login \
      --name "$CONTAINER_REGISTRY_NAME" \
      --expose-token > /dev/null
  }

  function docker_acr_login() {
    time_log "Login to $CONTAINER_REGISTRY_HOST using docker"

    SP_AIZEIO_PASSWORD="$(get_secret sp-aizeio-password)"

    docker login "$CONTAINER_REGISTRY_HOST" \
      --username "$SP_AIZEIO_ID" \
      --password "$SP_AIZEIO_PASSWORD"
  }

  export -f acr_login
  export -f docker_acr_login
  export -f hash_exists_for
  export -f get_secret
  export UTILS_LOADED
fi

if [ 'test_get_secret_ci' = "$1" ]; then
  KEY_VAULT="kv-aize-vcp-dev"
  export KEY_VAULT
  AZURE_TENANT_ID="05c09cca-3fe5-4786-93a6-f0cc31e7ec0b"
  export AZURE_TENANT_ID
  SP_K8S_VCP_ADMIN_ID="0ca3dcb8-6ece-4dda-a59f-25c1c89295e6"
  export SP_K8S_VCP_ADMIN_ID
  SP_K8S_VCP_ADMIN_PASSWORD="$(get_secret sp-k8s-vcp-admin-password)"
  export SP_K8S_VCP_ADMIN_PASSWORD
  UTILS_LOADED=''
  export UTILS_LOADED
  CI=true
  export CI

  shift
  source ./utils.sh

  EXPECTED='db_user@vcp-westeurope-dev-mysql'
  RESULT="$(get_secret mysql-administrator-login-username)"
  if [ "$RESULT" = "$EXPECTED" ]; then
    echo "PASSED"
    echo "======"
    echo "TOKEN=$RESULT"
  else
    echo "FAILED"
    echo "======"
    echo "EXPECTED=$EXPECTED"
    echo "RESULT  =$RESULT"
    exit 1
  fi
fi
