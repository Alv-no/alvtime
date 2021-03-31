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

resource "azurerm_cosmosdb_account" "this" {
  name                = var.name #Unique
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
  name                = var.name
  account_name        = azurerm_cosmosdb_account.this.name
  resource_group_name = azurerm_cosmosdb_account.this.resource_group_name
}

resource "azurerm_cosmosdb_mongo_collection" "this" {
  name                = "users"
  resource_group_name = azurerm_cosmosdb_account.this.resource_group_name
  database_name       = azurerm_cosmosdb_mongo_database.this.name
  account_name        = azurerm_cosmosdb_account.this.name

  index {
    keys = [
      "_id",
    ]
    unique = true
  }
}
