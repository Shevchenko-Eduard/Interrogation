# REST API

## fragment

**GET** - /api/v1/documents/{id_document}/fragments - получить данные о каждои фрагменте определенного документа

**GET** - /api/v1/documents/{id_document}/fragment/{id_fragment} - получить конкретный фрагмент конкретного документа

**POST** - /api/v1/documents/{id_document}/fragment - создать новый фрагмент

**DELETE** - /api/v1/documents/{id_document}/fragment/{id_fragment} - удалить фрагмент

## document

**GET** - /api/v1/documents - получить список документов без конкретных данных

**GET** - /api/v1/documents/{id} - получить более подробные данные но только для одного документа

**POST** - /api/v1/documents - создать новый документ

**PUT** - /api/v1/documents/{id} - изменить данные о документе

**DELETE** - /api/v1/documents/{id} - удалить документ

## encryption_type

**GET** - /api/v1/documents/encryption/types - получить список доступных типов шифрования документов

## secret

**GET** - /api/v1/documents/{id}/encryption/secret - получить ключ шифрования файла
