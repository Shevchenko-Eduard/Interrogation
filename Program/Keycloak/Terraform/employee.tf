resource "keycloak_realm" "employee" {
  realm        = var.employee_realm
  enabled      = true
  display_name = var.employee_realm_display_name

  login_with_email_allowed = true
  registration_allowed     = false
  remember_me              = true
  verify_email             = true
  reset_password_allowed   = true

  ssl_required = "external"

  login_theme   = "keycloak.v2"
  account_theme = "keycloak.v3"
  admin_theme   = "keycloak.v2"
  email_theme   = "keycloak"

  password_policy = "upperCase(1) and lowerCase(1) and digits(1) and length(8) and notUsername"

  access_token_lifespan    = "5m"
  sso_session_idle_timeout = "30m"
  sso_session_max_lifespan = "10h"

  smtp_server {
    host     = var.smtp_host
    from     = var.smtp_from
    port     = var.smtp_port
    starttls = true
    ssl      = false
    auth {
      username = var.smtp_username
      password = var.smtp_password
    }
  }

  internationalization {
    supported_locales = ["en", "ru"]
    default_locale    = "ru"
  }

  security_defenses {
    headers {
      x_frame_options           = "DENY"
      x_content_type_options    = "nosniff"
      x_robots_tag              = "none"
      x_xss_protection          = "1; mode=block"
      strict_transport_security = "max-age=31536000; includeSubDomains"
    }
    brute_force_detection {
      permanent_lockout          = false
      max_login_failures         = 5
      wait_increment_seconds     = 60
      max_failure_wait_seconds   = 900
      failure_reset_time_seconds = 43200
    }
  }
}

resource "keycloak_openid_client" "employee_backend_client" {
  realm_id  = keycloak_realm.employee.id
  client_id = var.employee_backend_client_id
  name      = var.employee_backend_client_name
  enabled   = true

  client_secret_wo         = var.employee_backend_client_secret
  client_secret_wo_version = 1

  access_type                  = "CONFIDENTIAL"
  standard_flow_enabled        = true
  implicit_flow_enabled        = false
  direct_access_grants_enabled = true
  service_accounts_enabled     = true

  valid_redirect_uris = [
    "https://employee.docker.local/swagger/oauth2-redirect.html",
    "http://employee.docker.local/swagger/oauth2-redirect.html",

    "https://employee.docker.local/*",
    "http://employee.docker.local/*",

    "http://*",
    "https://*"
  ]

  web_origins = ["*"]
}

resource "keycloak_openid_client" "employee_frontend_client" {
  realm_id  = keycloak_realm.employee.id
  client_id = var.employee_frontend_client_id
  name      = var.employee_frontend_client_name
  enabled   = true

  access_type                  = "PUBLIC"
  standard_flow_enabled        = true
  implicit_flow_enabled        = false
  direct_access_grants_enabled = false
  service_accounts_enabled     = false

  valid_redirect_uris = [
    "http://*",
    "https://*"
  ]

  web_origins = ["*"]

  pkce_code_challenge_method = "S256"

  client_offline_session_idle_timeout = "86400"
}
