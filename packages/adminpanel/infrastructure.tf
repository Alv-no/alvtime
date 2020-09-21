variable "git_hash" {
  description = "git hash"
  type        = string
}

variable "subscription_id" {
  description = "git subscription_id"
  type        = string
}

variable "service_name" {
  description = "service_name"
  type        = string
}

terraform {
  backend "azurerm" {}
}

provider "azurerm" {
  subscription_id = var.subscription_id
  tenant_id       = "76749190-4427-4b08-a3e4-161767dd1b73"
  version         = "=2.20.0"
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = var.service_name
  location = "West Europe"
}

resource "azurerm_app_service_plan" "asp" {
  name                = "${azurerm_resource_group.rg.name}-asp"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "Linux"
  reserved            = true

  sku {
    tier = "Basic"
    size = "B1"
  }
}

resource "azurerm_app_service" "as" {
  name = "${azurerm_resource_group.rg.name}-as"
  #checkov:skip=CKV_AZURE_13:Should be addressed properly
  #checkov:skip=CKV_AZURE_14:Should be addressed properly
  #checkov:skip=CKV_AZURE_16:Should be addressed properly
  #checkov:skip=CKV_AZURE_17:Should be addressed properly
  #checkov:skip=CKV_AZURE_18:Should be addressed properly
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.asp.id

  site_config {
    app_command_line = ""
    linux_fx_version = "DOCKER|alvnoas/${azurerm_resource_group.rg.name}:${var.git_hash}"
  }
}
