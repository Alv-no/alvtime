terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.48"
    }
  }

  backend "azurerm" {
    key = "stage-1.tfstate"
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${var.name}-${var.env}-${var.location}"
  location = var.location
}

data "azurerm_subscription" "current" {}

module "kubernetes_cluster" {
  source = "../kubernetes"

  name                            = "aks-${var.name}-${var.env}-${var.location}"
  location                        = var.location
  dns_prefix                      = "${var.name}-k8s"
  sp_alvtime_admin_client_id      = var.sp_alvtime_admin_client_id
  sp_alvtime_admin_rbac_secret    = var.sp_alvtime_admin_rbac_secret
  resource_group_name             = azurerm_resource_group.this.name
  sp_alvtime_admin_prod_object_id = var.sp_alvtime_admin_prod_object_id
}

module "mongo_db" {
  source = "../mongodb"

  name                = "cosmos-${var.name}-${var.env}"
  location            = var.location
  resource_group_name = azurerm_resource_group.this.name
}

module "sql_db" {
  source = "../sqldb"

  sql_server_name                         = "sql-${var.name}-${var.env}-${var.location}"
  sql_database_name                       = "sqldb-${var.name}-${var.env}"
  location                                = var.location
  sql_server_administrator_login          = var.sql_server_administrator_login
  sql_server_administrator_login_password = var.sql_server_administrator_login_password
  resource_group_name                     = azurerm_resource_group.this.name
}

data "azurerm_key_vault" "this" {
  name                = "alvtimetest"
  resource_group_name = "rg-alvtime-common"
}

resource "azurerm_key_vault_secret" "mongo-db-connection-string" {
  name = "mongo-db-connection-string"
  value = module.mongo_db.mongo_db_connection_strings[0]
  key_vault_id = data.azurerm_key_vault.this.id
}

resource "azurerm_key_vault_secret" "mongo-db-primary-key" {
  name = "mongo-db-primary-key"
  value = module.mongo_db.mongo_db_primary_key
  key_vault_id = data.azurerm_key_vault.this.id
}

resource "azurerm_key_vault_secret" "sql-connection-string" {
  name = "sql-connection-string"
  value = "Server=tcp:${module.sql_db.sql_server_name}.database.windows.net,1433;Initial Catalog=${module.sql_db.sql_database_name};Persist Security Info=False;User ID=${var.sql_server_administrator_login};Password=${var.sql_server_administrator_login_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = data.azurerm_key_vault.this.id
}



