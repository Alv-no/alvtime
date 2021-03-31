variable "hostname" {
  type        = string
  description = "Hostname"
}

variable "name" {
  type        = string
  description = "Name given to the cluster of services"
}

variable "resource_group_name" {
  type        = string
  description = "resource group name"
}

variable "sql_server_name" {
  type        = string
  description = "SQL database name"
}

variable "api_sql_firewall_rule_ip" {
  type        = string
  description = "IP that the api service connects to the sql dateabase from (available after a full deploy)"
}

variable "ip_address" {
  type        = string
  description = "List of ip addresses that the A records should point to"
}
