#!/bin/bash

touch ./Program/.env
ln -s ../.env ./Program/Backend/.env
ln -s ../.env ./Program/Frontend/.env
ln -s ../.env ./Program/Nginx/.env
ln -s ../.env ./Program/Keycloak/.env

cat << EOF > ./Program/.env
SERVER_DOMAIN="docker.local"

# --- keycloak

# keycloak db
KC_POSTGRES_DB="keycloak"
KC_POSTGRES_USER="keycloak"
KC_POSTGRES_PASSWORD="keycloak"
KC_POSTGRES_PORT=5435

# keycloak
KEYCLOAK_HTTP_PORT=7080
KEYCLOAK_HOST="auth.\${SERVER_DOMAIN}"
ADMIN_USERNAME="admin"
ADMIN_PASSWORD="admin"

# terraform
KC_REALM_NAME="employee"
KC_REALM_DISPLAY_NAME="employee"

KC_BACKEND_CLIENT_ID="Interrogation-api"
KC_BACKEND_CLIENT_NAME="Interrogation"
KC_BACKEND_CLIENT_SECRET="employee-secret"

KC_FRONTEND_CLIENT_ID="Interrogation"
KC_FRONTEND_CLIENT_NAME="Interrogation"

KC_SMPT_HOST="smtp.gmail.com"
KC_SMPT_PORT=587
KC_SMPT_USERNAME="__________@gmail.com"
KC_SMPT_FROM=\${KC_SMPT_USERNAME}
KC_SMPT_PASSWORD="__________"

# --- server

# server db
SERVER_POSTGRES_DB="hotel"
SERVER_POSTGRES_USER="hotel"
SERVER_POSTGRES_PASSWORD="hotel"
SERVER_POSTGRES_PORT=5433

# auth with keycloak
KEYCLOAK_AUTH_SERVER_URL=https://\${KEYCLOAK_HOST}/

# asp.net core
API_PORT=5252
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_SCHEMA=http
ASPNETCORE_HOST=*
ASPNETCORE_PORT=8000
ASPNETCORE_URLS=\${ASPNETCORE_SCHEMA}://\${ASPNETCORE_HOST}:\${ASPNETCORE_PORT}

MINIO_HOST=minio

# minio
S3_PORT=9000
S3_PORT_UI=9010
S3_HOST=0.0.0.0
MINIO_ROOT_PASSWORD=minio-password
MINIO_ROOT_USER=minio

# --- Nginx

# nginx
NGINX_HTTP_PORT=80
NGINX_HTTPS_PORT=443
NGINX_DOMAIN=\${SERVER_DOMAIN}

EOF