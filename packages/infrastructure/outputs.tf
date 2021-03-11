output "resource_group_name" {
  value = azurerm_resource_group.this.name
}

output "kubernetes_cluster_name" {
  value = azurerm_kubernetes_cluster.this.name
}

output "sql_server_name" {
  value = azurerm_sql_server.this.name
}

output "sql_database_name" {
  value = azurerm_sql_database.this.name
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
