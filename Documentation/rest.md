# REST API

## fragment

### **GET** - /v1/documents/{id}/fragments

Получить данные о каждои фрагменте определенного документа

### **GET** - /v1/documents/{<addr title="document id">***did***</addr>}/fragment/{<addr title="fragment id">***fid***</addr>}

Получить конкретный фрагмент конкретного документа

### **POST** - /v1/documents/{id}/fragment

Создать новый фрагмент

### **DELETE** - /v1/documents/{<addr title="document id">***did***</addr>}/fragment/{<addr title="fragment id">***fid***</addr>}

Удалить фрагмент

## document

### **GET** - /v1/documents

Получить список документов без конкретных данных

### **GET** - /v1/documents/{id}

Получить более подробные данные но только для одного документа

### **POST** - /v1/documents

Создать новый документ

### **PUT** - /v1/documents/{id}

Изменить данные о документе

### **DELETE** - /v1/documents/{id}

Удалить документ

## encryption_type

### **GET** - /v1/documents/encryption/types

Получить список доступных типов шифрования документов

## secret

### **GET** - /v1/documents/encryption/secrets/{<addr title="secret id">***sid***</addr>}

Получить ключ шифрования файла

### **POST** - /v1/documents/encryption/secrets

Создать ключ шифрования

### **DELETE** - /v1/documents/encryption/secrets/{<addr title="secret id">***sid***</addr>}

Удалить ключ шифрования
