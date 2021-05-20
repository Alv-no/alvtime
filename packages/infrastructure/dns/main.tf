terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "2.48"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_dns_zone" "this" {
  name                = var.hostname
  resource_group_name = var.resource_group_name
}

resource "azurerm_dns_a_record" "dnsarecordtest" {
  name                = "@"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_dns_zone.this.resource_group_name
  ttl                 = 3600
  records             = [var.ip_address]
}

resource "azurerm_dns_a_record" "dnsarecordtestwww" {
  name                = "www"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_dns_zone.this.resource_group_name
  ttl                 = 3600
  records             = [var.ip_address]
}

resource "azurerm_dns_a_record" "dnsarecordadmin" {
  name                = "admin"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_dns_zone.this.resource_group_name
  ttl                 = 3600
  records             = [var.ip_address]
}

resource "azurerm_dns_a_record" "dnsarecordslack" {
  name                = "slack-app"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_dns_zone.this.resource_group_name
  ttl                 = 3600
  records             = [var.ip_address]
}

resource "azurerm_dns_a_record" "dnsarecordapi" {
  name                = "api"
  zone_name           = azurerm_dns_zone.this.name
  resource_group_name = azurerm_dns_zone.this.resource_group_name
  ttl                 = 3600
  records             = [var.ip_address]
}

