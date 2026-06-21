# Interrogation

## Содержание

- [Стек технологий](#стек-технологий)
- [Деплой](#деплой)
  - [Предварительные требования](#предварительные-требования)
  - [Инструкция по развертыванию](#инструкция-по-развертыванию)
  - [Настройка переменных окружения](#настройка-переменных-окружения)
  - [Доступ к сервисам](#доступ-к-сервисам)
  - [Настройка локального хоста](#настройка-локального-хоста)

## Стек технологий

**Backend:**

- .NET 10.0 (C#)
- ASP.NET Core
- Entity Framework Core

**Frontend:**

- Avalonia (кроссплатформенное десктоп приложение)

**Инфраструктура:**

- PostgreSQL 18 (базы данных)
- MinIO (S3-совместимое хранилище)
- Keycloak 26.6.1 (аутентификация и авторизация)
- Nginx 1.30 (обратный прокси)
- Terraform 1.15.1 (Infrastructure as Code)

**Контейнеризация:**

- Docker
- Docker Compose

## Деплой

### Предварительные требования

**Поддерживаемые ОС:**

- Linux (Ubuntu 20.04+, Debian 11+, CentOS 8+, RHEL 8+, Fedora 35+)
- macOS 11+ (Intel и Apple Silicon)
- Windows 10/11 (с WSL 2)

**Программное обеспечение:**

- Docker Engine 20.10+ (скачать с [docker.com](https://www.docker.com/products/docker-desktop))
- Docker Compose 2.0+ (обычно идет в комплекте с Docker Desktop)

Проверить установку:

```bash
docker --version
docker-compose --version
```

**Системные требования:**

- **Минимум:** 4 CPU ядра, 8 GB RAM, 30 GB свободного дискового пространства
- **Рекомендуется:** 8 CPU ядер, 16 GB RAM, 50 GB свободного дискового пространства
*Примечание: Keycloak требует 2 CPU и 2 GB RAM, PostgreSQL базы данных требуют 512 MB каждая*

### Инструкция по развертыванию

1. **Создание файла окружения**

   ```bash
   ./create_env.sh
   ```

   Этот скрипт создаст файл `.env` с шаблоном переменных окружения.

2. **Запуск приложения**

   ```bash
   cd Program
   docker-compose up -d
   ```

3. **Остановка приложения**

   ```bash
   docker-compose down
   ```

### Настройка переменных окружения

После выполнения скрипта `create_env.sh` необходимо отредактировать файл `Program/.env` и установить реальные значения параметров.

**Обязательные параметры:**

- **SERVER_DOMAIN** - ваш домен (по умолчанию `docker.local` для локального тестирования)
- **ADMIN_USERNAME / ADMIN_PASSWORD** - учетные данные администратора Keycloak
- **SERVER_POSTGRES_PASSWORD** - пароль базы данных приложения
- **KC_POSTGRES_PASSWORD** - пароль базы данных Keycloak
- **MINIO_ROOT_PASSWORD** - пароль для хранилища MinIO

**Опциональные параметры для SMTP (отправка писем):**

- **KC_SMPT_HOST** - адрес SMTP сервера (по умолчанию smtp.gmail.com)
- **KC_SMPT_PORT** - порт SMTP (по умолчанию 587)
- **KC_SMPT_USERNAME** - email адрес для отправки
- **KC_SMPT_PASSWORD** - пароль приложения (не пароль аккаунта!)

**Пример заполнения для локального тестирования:**

```bash
SERVER_DOMAIN="docker.local"
ADMIN_USERNAME="admin"
ADMIN_PASSWORD="SecurePassword123!"
SERVER_POSTGRES_PASSWORD="db_password_123"
KC_POSTGRES_PASSWORD="keycloak_db_pass"
MINIO_ROOT_PASSWORD="minio_secure_pass"
```

### Доступ к сервисам

После запуска приложение будет доступно по адресу, указанному в переменной `SERVER_DOMAIN`:

- **Фронтенд**: `https://{SERVER_DOMAIN}`
- **API**: `https://api.{SERVER_DOMAIN}`
- **Keycloak (Auth)**: `https://auth.{SERVER_DOMAIN}`

> Примечание: для локального доступа по `docker.local` убедитесь, что настроена маршрутизация в `/etc/hosts` или на системном уровне

### Настройка локального хоста

Для тестирования используйте скрипт:

```bash
./test_host.sh
```

Чтобы добавить локальный хост на вашу систему:

**Linux/macOS:**

```bash
sudo nano /etc/hosts
```

Добавьте строку:

```txt
127.0.0.1 docker.local api.docker.local auth.docker.local
```

Сохраните (Ctrl+X, Y, Enter)

**Windows:**

- Откройте `Notepad` от имени администратора
- Откройте файл: `C:\Windows\System32\drivers\etc\hosts`
- Добавьте строку:

```txt
127.0.0.1 docker.local api.docker.local auth.docker.local
```

- Сохраните файл
