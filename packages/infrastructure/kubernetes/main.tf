terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.63.0"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_kubernetes_cluster" "this" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "${var.name}-k8s"
  sku_tier            = "Free"
  kubernetes_version  = "1.20.7"

  default_node_pool {
    name                 = "default"
    node_count           = 2
    vm_size              = "Standard_D2_v2"
    os_disk_size_gb      = 30
    orchestrator_version = "1.20.7"
  }

  service_principal {
    client_id     = var.sp_alvtime_admin_client_id
    client_secret = var.sp_alvtime_admin_rbac_secret
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

data "azurerm_subscription" "current" {
}

resource "azurerm_container_registry" "this" {
  count = length(var.sp_alvtime_admin_prod_object_id) > 0 ? 1 : 0

  name                = "acralvtime"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "Basic"
}

resource "azurerm_role_assignment" "acrpull_role" {
  count = length(var.sp_alvtime_admin_prod_object_id) > 0 ? 1 : 0

  scope                            = "/subscriptions/${data.azurerm_subscription.current.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.ContainerRegistry/registries/${azurerm_container_registry.this[0].name}"
  role_definition_name             = "AcrPull"
  principal_id                     = var.sp_alvtime_admin_prod_object_id
  skip_service_principal_aad_check = true
}
