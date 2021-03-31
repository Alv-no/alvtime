variable "name" {
  type        = string
  description = "Name given to kubernetes cluster"
}

variable "location" {
  type        = string
  description = "Primary Location"
}

variable "dns_prefix" {
  type        = string
  description = "DNS prefix"
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

variable "resource_group_name" {
  type        = string
  description = "Resource group name"
}
