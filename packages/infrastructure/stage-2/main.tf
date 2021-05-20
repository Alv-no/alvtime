terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "2.48"
    }
  }

  backend "azurerm" {
    key = "stage-3.tfstate"
  }
}

provider "azurerm" {
  features {}
}

module "dns" {
  source = "../dns"

  hostname            = var.hostname
  ip_address          = var.ip_address
  resource_group_name = var.resource_group_name
}

resource "azurerm_sql_firewall_rule" "api" {
  name                = "${var.name}-api"
  resource_group_name = var.resource_group_name
  server_name         = var.sql_server_name
  start_ip_address    = var.api_sql_firewall_rule_ip
  end_ip_address      = var.api_sql_firewall_rule_ip
}

