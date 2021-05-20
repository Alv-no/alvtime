output "kubernetes_cluster_name" {
  value = azurerm_kubernetes_cluster.this.name
}

output "effective_outbound_ips" {
  value = azurerm_kubernetes_cluster.this.network_profile[0].load_balancer_profile[0].effective_outbound_ips
}
