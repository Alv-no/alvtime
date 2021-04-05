#!/usr/bin/env bash

set -e

cd packages/infrastructure/stage-1

terraform init \
  -input=false \
  -backend-config="../$ENV-backend.tfvars" \

terraform apply -input=false plan

RESOURCE_GROUP_NAME="$(terraform output -raw resource_group_name)"
KUBERNETES_CLUSTER_NAME="$(terraform output -raw kubernetes_cluster_name)"
SUBSCRIPTION_ID="$(terraform output -raw subscription_id)"
TENANT_ID="$(terraform output -raw tenant_id)"

# Retrieve the access credentials for your cluster and automatically configure kubectl
az aks get-credentials \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --name "$KUBERNETES_CLUSTER_NAME" \

cd ../../charts

# Add the ingress-nginx repository
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx

# Use Helm to deploy an NGINX ingress controller
helm upgrade nginx-ingress ingress-nginx/ingress-nginx \
    --install \
    --set controller.replicaCount=2 \
    --set controller.nodeSelector."beta\.kubernetes\.io/os"=linux \
    --set defaultBackend.nodeSelector."beta\.kubernetes\.io/os"=linux \
    --set controller.admissionWebhooks.patch.nodeSelector."beta\.kubernetes\.io/os"=linux

# Add the Jetstack Helm repository
helm repo add jetstack https://charts.jetstack.io

# Update your local Helm chart repository cache
helm repo update

# Install the cert-manager Helm chart
helm upgrade cert-manager jetstack/cert-manager \
  --install \
  --set installCRDs=true \
  --set nodeSelector."kubernetes\.io/os"=linux \
  --set webhook.nodeSelector."kubernetes\.io/os"=linux \
  --set cainjector.nodeSelector."kubernetes\.io/os"=linux

# Install letsencrypt cluster issuer
helm upgrade cluster-issuer letsencryptClusterIssuer \
  --install \
  --set email="$CERT_EMAIL" \
  --set client.id="$SP_ALVTIME_ADMIN_CLIENT_ID" \
  --set client.secret="$SP_ALVTIME_ADMIN_RBAC_SECRET" \
  --set subscriptionID="$SUBSCRIPTION_ID" \
  --set tenantID="$TENANT_ID" \
  --set resourceGroupName="$RESOURCE_GROUP_NAME" \
  --set hostedZoneName="$HOSTNAME" \

cd ../infrastructure/stage-2

terraform init \
  -input=false \
  -backend-config="../$ENV-backend.tfvars" \

terraform apply -input=false plan

cd ../../..
