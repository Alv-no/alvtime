#!/usr/bin/env bash

set -e

ENV=$(echo "$1" | awk '{print tolower($0)}')
KEY_VAULT="alvtimedev"

function getSecret() {
  az keyvault secret show --vault-name $KEY_VAULT --name $1 | jq '.value' -r
}

export NGROK_AUTH="$(getSecret ngrok-auth)"
export AZURE_AD_CLIENT_SECTRET="$(getSecret azure-ad-client-sectret)"
export HOST="$(getSecret host)"
export NGROK_HOSTNAME="$(getSecret ngrok-hostname)"
export SLACK_BOT_TOKEN="$(getSecret slack-bot-token)"
export SLACK_SIGNING_SECRET="$(getSecret slack-signing-secret)"

docker-compose up
