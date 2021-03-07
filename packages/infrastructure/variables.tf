variable "location" {
  type        = string
  description = "Primary Location"
  default     = "westeurope"
}

variable "prefix" {
  type        = string
  description = "prefix"
  default     = "alvtime"
}

variable "client_id" {
  type = string
}

variable "client_secret" {
  type = string
}
