# Backend API terraform module

variable "name" {
  type = string
}
variable "location" {
	type = string
}
variable "resource_group_name" {
	type = string
}
variable "sql_server_administrator_login" {
  type = string
  description = "Chosen SQL dateabase server admin username"
}
variable "sql_server_administrator_login_password" {
  type      = string
  description = "Chosen SQL dateabase server admin password"
  sensitive = true
}
variable "api_sql_firewall_rule_ip" {
  type = string
  description = "IP that the api service connects to the sql dateabase from (available after a full deploy)"
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "2.48"
    }
  }
}

resource "azurerm_sql_server" "this" {
  name                         = "sql-backend-${var.name}"
  administrator_login          = var.sql_server_administrator_login
  administrator_login_password = var.sql_server_administrator_login_password
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"

  timeouts {}
}

resource "azurerm_sql_firewall_rule" "api" {
  count               = var.api_sql_firewall_rule_ip != "" ? 1 : 0
  name                = "backend-api"
  resource_group_name = var.resource_group_name
  server_name         = azurerm_sql_server.this.name
  start_ip_address    = var.api_sql_firewall_rule_ip
  end_ip_address      = var.api_sql_firewall_rule_ip
}

resource "azurerm_sql_database" "this" {
  name                = "sqldb-backend-${var.name}"
  server_name         = azurerm_sql_server.this.name
  resource_group_name = var.resource_group_name
  location            = var.location
  create_mode         = "Default"
}

output "sql_server_name" {
  value = azurerm_sql_server.this.name
}

output "sql_database_name" {
  value = azurerm_sql_database.this.name
}
