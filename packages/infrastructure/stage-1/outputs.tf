output "resource_group_name" {
  value = azurerm_resource_group.this.name
}

output "sql_server_name" {
  value = module.sql_db.sql_server_name
}

output "sql_database_name" {
  value = module.sql_db.sql_database_name
}

output "mongo_db_name" {
  value = module.mongo_db.mongo_db_name
}

output "mongo_db_connection_strings" {
  sensitive = true
  value     = module.mongo_db.mongo_db_connection_strings
}

output "mongo_db_primary_key" {
  sensitive = true
  value     = module.mongo_db.mongo_db_primary_key
}

output "tenant_id" {
  value = data.azurerm_subscription.current.tenant_id
}

output "subscription_id" {
  value = data.azurerm_subscription.current.subscription_id
}

output "kubernetes_cluster_name" {
  value = module.kubernetes_cluster.kubernetes_cluster_name
}

output "effective_outbound_ips" {
  value = module.kubernetes_cluster.effective_outbound_ips
}
