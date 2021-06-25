output "resource_group_name" {
  value = azurerm_resource_group.this.name
}

output "sql_server_name" {
  value = module.sql_db.sql_server_name
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
