# --- DOCUMENT ----

resource "keycloak_role" "document_create" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "DocumentCreate"
}

resource "keycloak_role" "document_update" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "DocumentUpdate"
}

resource "keycloak_role" "document_delete" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "DocumentDelete"
}

resource "keycloak_role" "documents_read" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "DocumentsRead"
}

resource "keycloak_role" "document_read_by_id" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "DocumentReadById"
}

resource "keycloak_role" "document_download_by_id" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "DocumentDownloadById"
}

# --- ENCRYPTION TYPE ----

resource "keycloak_role" "encryption_type_read" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "EncryptionTypeRead"
}

resource "keycloak_role" "encryption_type_read_by_id" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "EncryptionTypeReadById"
}

# --- FRAGMENT ----

resource "keycloak_role" "fragment_create" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "FragmentCreate"
}

resource "keycloak_role" "fragment_delete" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "FragmentDelete"
}

resource "keycloak_role" "fragment_read_by_id" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "FragmentReadById"
}

resource "keycloak_role" "fragment_read_by_document_id" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "FragmentReadByDocumentId"
}

# --- SECRET ----

resource "keycloak_role" "secret_create" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "SecretCreate"
}

resource "keycloak_role" "secret_delete" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "SecretDelete"
}

resource "keycloak_role" "secret_read_by_id" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "SecretReadById"
}