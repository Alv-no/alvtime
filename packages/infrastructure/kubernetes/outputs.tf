output "kubernetes_cluster_name" {
  value = azurerm_kubernetes_cluster.this.name
}

output "kubernetes_cluster_host" {
  sensitive = true
  value     = azurerm_kubernetes_cluster.this.kube_config.0.host
}

output "kubernetes_cluster_client_key" {
  sensitive = true
  value     = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.client_key)
}

output "kubernetes_cluster_client_certificate" {
  sensitive = true
  value     = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.client_certificate)
}

output "kubernetes_cluster_ca_certificate" {
  sensitive = true
  value     = base64decode(azurerm_kubernetes_cluster.this.kube_config.0.cluster_ca_certificate)
}

output "effective_outbound_ips" {
  value = azurerm_kubernetes_cluster.this.network_profile[0].load_balancer_profile[0].effective_outbound_ips
}
