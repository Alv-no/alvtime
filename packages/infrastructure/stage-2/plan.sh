#!/usr/bin/env bash

set -e

cd ../stage-1

KUBERNETES_CLUSTER_NAME="$(terraform output -raw kubernetes_cluster_name)"
RESOURCE_GROUP_NAME="$(terraform output -raw resource_group_name)"
SQL_SERVER_NAME="$(terraform output -raw sql_server_name)"
EGRESS_IP="$(az network public-ip show --ids $(terraform output -json effective_outbound_ips | jq '.[0]' -r) | jq '.ipAddress' -r)"

cd ../stage-2

# Retrieve the access credentials for your cluster and automatically configure kubectl
az aks get-credentials \
  --resource-group $RESOURCE_GROUP_NAME \
  --name $KUBERNETES_CLUSTER_NAME \

KUBERNETES_CLUSTER_PUBLIC_IP="$(kubectl get services nginx-ingress-ingress-nginx-controller | grep LoadBalancer | awk '{print $4}')"

terraform init \
  -input=false \
  -backend-config="../$ENV-backend.tfvars" \
  -upgrade \

  terraform validate

terraform plan \
  -input=false \
  -var hostname="$HOSTNAME" \
  -var name="$PROJECT" \
  -var api_sql_firewall_rule_ip="$EGRESS_IP" \
  -var sql_server_name="$SQL_SERVER_NAME" \
  -var ip_address="$KUBERNETES_CLUSTER_PUBLIC_IP" \
  -var resource_group_name="$RESOURCE_GROUP_NAME" \
  -out=plan \
