variable "hostname" {
  type        = string
  description = "Hostname"
}

variable "ip_address" {
  type        = string
  description = "List of ip addresses that the A records should point to"
}

variable "resource_group_name" {
  type        = string
  description = "Resource group name"
}
