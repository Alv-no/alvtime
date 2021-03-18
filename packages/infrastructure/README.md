# Infrastructure

## Dependencies

- [azure cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) version 2.20.0 or later
- [terraform cli](https://learn.hashicorp.com/tutorials/terraform/install-cli) version 0.14.0 or later

# How to provision infrastructure

1. Login to azure using azure cli `az login`
1. Set the subscription you would like to deploy to as default `az account set --subscription <name or id>`
1. Create a `terraform.tfvars` file and fill it with correct values.

```
env = "test"
state_storage_account_name = ""
aks_service_principal_client_id = ""
aks_service_principal_client_secret = ""
sql_server_administrator_login = ""
sql_server_administrator_login_password = ""
api_sql_firewall_rule_ip = "0.0.0.0"
```

1. In this directory run:
   1. `terraform init -backend-config="test-backend.tfvars"` to download terraform dependencies
   1. `terraform plan` to see what actions terraform will take on the infrastructure if `terraform apply` is ran.
   1. `terraform apply` to apply changes
