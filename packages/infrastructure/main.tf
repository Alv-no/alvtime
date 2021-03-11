resource "azurerm_resource_group" "this" {
  name     = "rg-${var.name}-${var.env}-${var.location}"
  location = var.location
}

resource "azurerm_cosmosdb_account" "slack_db" {
  name                = "cosmos-${var.name}-${var.env}" #Unique
  enable_free_tier    = var.cosmosdb_enable_free_tier
  kind                = "MongoDB"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  offer_type          = "Standard"

  geo_location {
    failover_priority = 0
    location          = azurerm_resource_group.this.location
    zone_redundant    = false
  }

  tags = {
    "CosmosAccountType"       = "Non-Production"
    "defaultExperience"       = "Azure Cosmos DB for MongoDB API"
    "hidden-cosmos-mmspecial" = ""
  }

  capabilities {
    name = "DisableRateLimitingResponses"
  }

  capabilities {
    name = "EnableMongo"
  }

  consistency_policy {
    consistency_level       = "Session"
    max_interval_in_seconds = 5
    max_staleness_prefix    = 100
  }

  timeouts {}
}

resource "azurerm_cosmosdb_mongo_database" "this" {
  name                = "cosmos-${var.name}-${var.env}"
  account_name        = azurerm_cosmosdb_account.slack_db.name
  resource_group_name = azurerm_cosmosdb_account.slack_db.resource_group_name
}

resource "azurerm_cosmosdb_mongo_collection" "this" {
  name                = "users"
  resource_group_name = azurerm_cosmosdb_account.slack_db.resource_group_name
  database_name       = azurerm_cosmosdb_mongo_database.this.name
  account_name        = azurerm_cosmosdb_account.slack_db.name

  index {
    keys = [
      "_id",
    ]
    unique = true
  }
}

resource "azurerm_sql_server" "this" {
  name                         = "sql-${var.name}-${var.env}-${var.location}" # Unique
  administrator_login          = var.sql_server_administrator_login
  administrator_login_password = var.sql_server_administrator_login_password
  resource_group_name          = azurerm_resource_group.this.name
  location                     = azurerm_resource_group.this.location
  version                      = "12.0"

  timeouts {}
}

resource "azurerm_sql_firewall_rule" "api" {
  name                = "${var.name}-api"
  resource_group_name = azurerm_resource_group.this.name
  server_name         = azurerm_sql_server.this.name
  start_ip_address    = var.api_sql_firewall_rule_ip
  end_ip_address      = var.api_sql_firewall_rule_ip
}

resource "azurerm_sql_database" "this" {
  name                = "sqldb-${var.name}-${var.env}"
  server_name         = azurerm_sql_server.this.name
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  create_mode         = "Default"
}

resource "azurerm_kubernetes_cluster" "this" {
  name                = "aks-${var.name}-${var.env}-${var.location}"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  dns_prefix          = "${var.name}-k8s"

  default_node_pool {
    name            = "default"
    node_count      = var.aks_node_count
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

resource "helm_release" "nginx_ingress" {
  name       = "nginx-ingress"
  repository = "https://kubernetes.github.io/ingress-nginx"
  chart      = "ingress-nginx"

  set {
    name  = "controller.replicaCount"
    value = "1"
    type  = "string"
  }

  set {
    name  = "controller.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "defaultBackend.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "controller.admissionWebhooks.patch.nodeSelector.beta\\.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }
}

resource "helm_release" "cert-manager" {
  name       = "cert-manager"
  repository = "https://charts.jetstack.io"
  chart      = "cert-manager"
  version    = "v0.16.1"

  set {
    name  = "installCRDs"
    value = "true"
    type  = "string"
  }

  set {
    name  = "nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "webhook.nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }

  set {
    name  = "cainjector.nodeSelector.kubernetes\\.io/os"
    value = "linux"
    type  = "string"
  }
}
