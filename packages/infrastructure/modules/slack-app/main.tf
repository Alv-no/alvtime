# Slack app terraform module

variable "name" {
  type = string
}
variable "location" {
	type = string
}
variable "resource_group_name" {
	type = string
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "2.48"
    }
  }
}

resource "azurerm_cosmosdb_account" "slack_db" {
  name                = "cosmos-slackapp-${var.name}"
  enable_free_tier    = true
  kind                = "MongoDB"
  location            = var.location
  resource_group_name = var.resource_group_name
  offer_type          = "Standard"

  geo_location {
    failover_priority = 0
    location          = var.location
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
  name                = "cosmos-slackapp-${var.name}"
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

output "slack_db_name" {
  value = azurerm_cosmosdb_account.slack_db.name
}

output "slack_db_connection_strings" {
  sensitive = true
  value     = azurerm_cosmosdb_account.slack_db.connection_strings
}

output "slack_db_primary_key" {
  sensitive = true
  value     = azurerm_cosmosdb_account.slack_db.primary_key
}
