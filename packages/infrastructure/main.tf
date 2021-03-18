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
    host                   = module.kubernetes.kubernetes_host
    client_key             = module.kubernetes.kubernetes_client_key
    client_certificate     = module.kubernetes.kubernetes_client_certificate
    cluster_ca_certificate = module.kubernetes.kubernetes_cluster_ca_certificate
  }
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${local.name}"
  location = local.location
}

module "kubernetes" {
	source = "./modules/kubernetes"
	name                                = local.name
	location                            = local.location
	resource_group_name                 = azurerm_resource_group.this.name
	aks_service_principal_client_id     = var.aks_service_principal_client_id
	aks_service_principal_client_secret = var.aks_service_principal_client_secret
}

module "letsecrypt" {
	source = "./modules/letsencrypt"
}

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
