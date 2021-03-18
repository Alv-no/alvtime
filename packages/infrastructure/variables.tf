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
}

variable "aks_service_principal_client_id" {
  type    = string
  description = "Azure Active Directory App registration (service principal) id"
}

variable "aks_service_principal_client_secret" {
  type      = string
  description = "Azure Active Directory App registration (service principal) client secret"
  sensitive = true
}

variable "sql_server_administrator_login" {
  type = string
  description = "Chosen SQL dateabase server admin username"
}

variable "sql_server_administrator_login_password" {
  type      = string
  description = "Chosen SQL dateabase server admin password"
  sensitive = true
}

variable "api_sql_firewall_rule_ip" {
  type = string
  description = "IP that the api service connects to the sql dateabase from (available after a full deploy)"
  default = ""
}
