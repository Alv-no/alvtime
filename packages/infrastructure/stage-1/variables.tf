variable "location" {
  type        = string
  description = "Primary Location"
}

variable "name" {
  type        = string
  description = "Name given to the cluster of services"
}

variable "env" {
  type        = string
  description = "environment"
}

variable "sp_alvtime_admin_client_id" {
  type        = string
  description = "Azure Active Directory App registration (service principal) id"
}

variable "sp_alvtime_admin_rbac_secret" {
  type        = string
  description = "Azure Active Directory App registration (service principal) client secret"
  sensitive   = true
}

variable "sql_server_administrator_login" {
  type        = string
  description = "Chosen SQL dateabase server admin username"
}

variable "sql_server_administrator_login_password" {
  type        = string
  description = "Chosen SQL dateabase server admin password"
  sensitive   = true
}

variable "api_sql_firewall_rule_ip" {
  type        = string
  description = "IP that the api service connects to the sql dateabase from (available after a full deploy)"
  default     = "0.0.0.0"
}

variable "hostname" {
  type        = string
  description = "Hostname"
}

variable "sp_alvtime_admin_prod_object_id" {
  type        = string
  description = "Production Azure Active Directory App registration (service principal) id"
}
