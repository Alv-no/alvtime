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
    resource_group_name  = "Alvtime-common"
    storage_account_name = "terraformstatealvdev"
    container_name       = "terraformstate"
    key                  = "tf.tfstate"
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

