variable "location" {
  type        = string
  description = "Primary Location"
  default     = "westeurope"
}

variable "name" {
  type        = string
  description = "name"
  default     = "alvtime"
}

variable "env" {
  type        = string
  description = "environment"
  default     = "test"
}

variable "aks_service_principal_client_id" {
  type    = string
  default = ""
}

variable "aks_service_principal_client_secret" {
  type      = string
  default   = ""
  sensitive = true
}

variable "sql_server_administrator_login" {
  type    = string
  default = ""
}

variable "sql_server_administrator_login_password" {
  type      = string
  default   = ""
  sensitive = true
}

variable "api_sql_firewall_rule_ip" {
  type = string
}

variable "cosmosdb_enable_free_tier" {
  type    = bool
  default = true
}

variable "aks_node_count" {
  type    = number
  default = 2
}
