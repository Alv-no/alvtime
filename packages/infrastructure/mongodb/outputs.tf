output "mongo_db_name" {
  value = azurerm_cosmosdb_account.this.name
}

output "mongo_db_connection_strings" {
  sensitive = true
  value     = azurerm_cosmosdb_account.this.connection_strings
}

output "mongo_db_primary_key" {
  sensitive = true
  value     = azurerm_cosmosdb_account.this.primary_key
}

