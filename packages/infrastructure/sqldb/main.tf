terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.48"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_sql_server" "this" {
  name                         = var.sql_server_name # Unique
  administrator_login          = var.sql_server_administrator_login
  administrator_login_password = var.sql_server_administrator_login_password
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"

  timeouts {}
}

resource "azurerm_sql_database" "this" {
  name                = var.sql_database_name
  server_name         = azurerm_sql_server.this.name
  resource_group_name = azurerm_sql_server.this.resource_group_name
  location            = azurerm_sql_server.this.location
  create_mode         = "Default"
  edition             = "Basic"
}
