#!/usr/bin/env bash

# Requires:
# - curl
# - jq

# Follow this link to set up a azure ad app registration with
# a api permission for user_impersination on key vaults and a
# client secret
# https://www.c-sharpcorner.com/article/how-to-access-azure-key-vault-secrets-through-rest-api-using-postman/

TENANT=''
CLIENT_ID=''
CLIENT_SECRET=''
KEY_VAULT=''

function getToken() {
  curl "https://login.microsoftonline.com/$TENANT/oauth2/v2.0/token" \
    --request POST \
    --silent \
    --location \
    --header 'Content-Type: application/x-www-form-urlencoded' \
    --data-urlencode 'grant_type=client_credentials' \
    --data-urlencode 'scope=https://vault.azure.net/.default' \
    --data-urlencode "client_id=$CLIENT_ID" \
    --data-urlencode "client_secret=$CLIENT_SECRET" \
    | jq -r '.access_token'
}

function getSecret() {
  curl "https://$KEY_VAULT.vault.azure.net/secrets/$2?api-version=7.2" \
    --request GET \
    --silent \
    --location \
    --header "Authorization: Bearer $1" \
    | jq -r '.value'
}

TOKEN="$(getToken)"
getSecret "$TOKEN""$SECRET_NAME"
