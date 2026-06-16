terraform {
  required_providers {
    keycloak = {
      source  = "keycloak/keycloak"
      version = "5.7.0"
    }
  }
}

provider "keycloak" {
  client_id = "admin-cli"
  url       = var.kc_url
  username  = var.admin_username
  password  = var.admin_password
  realm     = "master"
}