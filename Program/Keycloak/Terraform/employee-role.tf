resource "keycloak_role" "hotel_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Hotel-Create"
}

resource "keycloak_role" "hotel_update_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Hotel-Update"
}

resource "keycloak_role" "hotel_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Hotel-Delete"
}

resource "keycloak_role" "hotel_hotel_tag_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "HotelHotelTag-Create"
}

resource "keycloak_role" "hotel_hotel_tag_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "HotelHotelTag-Delete"
}

resource "keycloak_role" "hotel_image_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "HotelImage-Create"
}

resource "keycloak_role" "hotel_image_update_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "HotelImage-Update"
}

resource "keycloak_role" "hotel_image_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "HotelImage-Delete"
}

resource "keycloak_role" "hotel_tag_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "HotelTag-Create"
}

resource "keycloak_role" "hotel_tag_update_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "HotelTag-Update"
}

resource "keycloak_role" "hotel_tag_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "HotelTag-Delete"
}

resource "keycloak_role" "payment_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Payment-Create"
}

resource "keycloak_role" "payment_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Payment-Delete"
}

resource "keycloak_role" "reservation_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Reservation-Create"
}

resource "keycloak_role" "reservation_update_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Reservation-Update"
}

resource "keycloak_role" "reservation_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Reservation-Delete"
}

resource "keycloak_role" "room_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Room-Create"
}

resource "keycloak_role" "room_update_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Room-Update"
}

resource "keycloak_role" "room_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "Room-Delete"
}

resource "keycloak_role" "room_image_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomImage-Create"
}

resource "keycloak_role" "room_image_update_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomImage-Update"
}

resource "keycloak_role" "room_image_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomImage-Delete"
}

resource "keycloak_role" "room_room_tag_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomRoomTag-Create"
}

resource "keycloak_role" "room_room_tag_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomRoomTag-Delete"
}

resource "keycloak_role" "room_tag_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomTag-Create"
}

resource "keycloak_role" "room_tag_update_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomTag-Update"
}

resource "keycloak_role" "room_tag_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomTag-Delete"
}

resource "keycloak_role" "room_type_create_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomType-Create"
}

resource "keycloak_role" "room_type_update_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomType-Update"
}

resource "keycloak_role" "room_type_delete_role" {
  realm_id    = keycloak_realm.employee.id
  client_id   = keycloak_openid_client.employee_backend_client.id
  name        = "RoomType-Delete"
}