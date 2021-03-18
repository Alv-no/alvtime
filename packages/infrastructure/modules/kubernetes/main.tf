# Kubernetes terraform module

variable "name" {
  type = string
}
variable "location" {
	type = string
}
variable "resource_group_name" {
	type = string
}
variable "aks_service_principal_client_id" {
  type        = string
  description = "Azure Active Directory App registration (service principal) id"
}
variable "aks_service_principal_client_secret" {
  type        = string
  description = "Azure Active Directory App registration (service principal) client secret"
  sensitive   = true
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "2.48"
    }
  }
}

resource "azurerm_kubernetes_cluster" "this" {
  name                = "aks-kubernetes-${var.name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "${var.name}-k8s"

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

output "kubernetes_cluster_name" {
  value = azurerm_kubernetes_cluster.this.name
}
output "kubernetes_host" {
	value = azurerm_kubernetes_cluster.this.kube_config.0.host
}
output "kubernetes_client_key" {
	value = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.client_key)
}
output "kubernetes_client_certificate" {
	value = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.client_certificate)
}
output "kubernetes_cluster_ca_certificate" {
	value = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.cluster_ca_certificate)
}
