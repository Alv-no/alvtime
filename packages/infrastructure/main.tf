locals {
	location = "westeurope"
	name = "alvtime-${var.env}-westeurope"
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "2.48"
    }
    helm = {
      source  = "hashicorp/helm"
      version = ">= 2.0.2"
    }
  }

  backend "azurerm" {
  }
}

provider "azurerm" {
  features {}
}

provider "helm" {
  kubernetes {
    host                   = azurerm_kubernetes_cluster.this.kube_config.0.host
    client_key             = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.client_key)
    client_certificate     = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.client_certificate)
    cluster_ca_certificate = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.cluster_ca_certificate)
  }
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${local.name}"
  location = local.location
}

resource "azurerm_kubernetes_cluster" "this" {
  name                = "aks-kubernetes-${local.name}"
	location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  dns_prefix          = "${local.name}-k8s"

  default_node_pool {
    name            = "default"
    node_count      = 2
    vm_size         = "Standard_D2_v2"
    os_disk_size_gb = 30
  }

  service_principal {
    client_id     = var.aks_service_principal_client_id
    client_secret = var.aks_service_principal_client_secret
  }

  role_based_access_control {
    enabled = true
  }

  addon_profile {
    kube_dashboard {
      enabled = false
    }
  }
}

resource "helm_release" "nginx_ingress" {
  name       = "nginx-ingress"
  repository = "https://kubernetes.github.io/ingress-nginx"
  chart      = "ingress-nginx"

  set {
    name  = "controller.replicaCount"
    value = "1"
    type  = "string"
  }

  set {
    name  = "controller.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "defaultBackend.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "controller.admissionWebhooks.patch.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }
}

resource "helm_release" "cert-manager" {
  name       = "cert-manager"
  repository = "https://charts.jetstack.io"
  chart      = "cert-manager"
  version    = "v0.16.1"

  set {
    name  = "installCRDs"
    value = "true"
    type  = "string"
  }

  set {
    name  = "nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "webhook.nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "cainjector.nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }
}

# Modules

module "api" {
	source = "./modules/api"
	name                                    = local.name
	location                                = local.location
	resource_group_name                     = azurerm_resource_group.this.name
	sql_server_administrator_login          = var.sql_server_administrator_login
	sql_server_administrator_login_password = var.sql_server_administrator_login_password
	api_sql_firewall_rule_ip                = var.api_sql_firewall_rule_ip
}

module "slack-app" {
	source = "./modules/slack-app"
	name                = local.name
	location            = local.location
	resource_group_name = azurerm_resource_group.this.name
}
