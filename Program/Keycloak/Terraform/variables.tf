// --- PROVIDER ---

variable "admin_username" {
  type      = string
  sensitive = true
}

variable "admin_password" {
  type      = string
  sensitive = true
}

variable "kc_url" {
  type = string
}

// --- EMPLOYEE ---

variable "employee_realm" {
  type = string
}

variable "employee_realm_display_name" {
  type    = string
}

// --- EMPLOYEE --- BACKEND --- 

variable "employee_backend_client_id" {
  type = string
}

variable "employee_backend_client_name" {
  type    = string
}

variable "employee_backend_client_secret" {
  type = string
}

// --- EMPLOYEE --- FRONTEND --- 

variable "employee_frontend_client_id" {
  type = string
}

variable "employee_frontend_client_name" {
  type    = string
}

// --- SMTP (для уведомлений) ---

variable "smtp_host" {
  type = string
}

variable "smtp_port" {
  type = number
}

variable "smtp_from" {
  type    = string
}

variable "smtp_username" {
  type      = string
  sensitive = true
}

variable "smtp_password" {
  type      = string
  sensitive = true
}
