variable "sql_server_name" {
  type        = string
  description = "Name given to sql server"
}

variable "sql_database_name" {
  type        = string
  description = "Name given to sql dateabase"
}

variable "location" {
  type        = string
  description = "Primary Location"
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

variable "resource_group_name" {
  type        = string
  description = "Resource group name"
}
