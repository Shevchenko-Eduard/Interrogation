# Interrogation Frontend

Avalonia-клиент для работы с протоколами допроса и зашифрованными фрагментами.

## Интеграция

- авторизация через Keycloak Authorization Code + PKCE;
- Keycloak: `https://auth.docker.local/realms/employee`;
- API: `https://api.docker.local`;
- получение списка через `GET /v1/documents`;
- скачивание через `GET /v1/documents/{id}/download`;
- роли берутся из `resource_access.Interrogation-api.roles` access token;
- локальное шифрование выполняется через OpenSSL.

Локальные тестовые пользователи и mock-список документов удалены. Пользователи, блокировки и ограничения входа находятся в Keycloak.

## Локальные функции

- импорт `.txt`, `.md`, `.docx`, `.odt`, `.enc` и `.enc.json`;
- шифрование документа или выбранного фрагмента;
- самодостаточный контейнер с HMAC-контролем целостности;
- постоянный журнал действий без открытого текста и паролей.
- поиск и фильтрация журнала;
- экспорт расшифрованного текста в DOCX и ODT;
- автоматический выход после 15 минут бездействия;
- обновление access token и завершение сессии Keycloak.

## Запуск

```powershell
dotnet run --project .\Interrogation.Client\Interrogation.Client.csproj
```

Для входа должны быть доступны Keycloak и DNS-имя `auth.docker.local`. После входа клиент загружает документы из backend API.

## Сборка

```powershell
dotnet build .\Interrogation.Frontend.slnx -c Release
```

Автономная Windows-публикация вместе с OpenSSL:

```powershell
.\publish-win-x64.ps1
```

## Ограничение API

Безопасная серверная загрузка пока не включена: `POST /v1/documents` требует `SecretId`, но ответы `GET /v1/documents` и `GET /v1/documents/{id}` не возвращают `SecretId`. До исправления backend DTO клиент не подставляет фиктивные идентификаторы и сохраняет шифрование локальным.
