#!/bin/bash

# Цвета для вывода
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Путь к файлу .env
ENV_FILE="./Program/.env"

# Проверка существования файла .env
if [ ! -f "$ENV_FILE" ]; then
    echo -e "${RED}Ошибка: Файл .env не найден по пути $ENV_FILE${NC}"
    echo -e "${YELLOW}Пожалуйста, сначала запустите скрипт create_env.sh для создания файла .env${NC}"
    exit 1
fi

# Чтение переменной SERVER_DOMAIN из .env файла
SERVER_DOMAIN=$(grep -E "^SERVER_DOMAIN=" "$ENV_FILE" | cut -d '=' -f2- | tr -d '"' | tr -d "'")

# Проверка, что переменная найдена и не пуста
if [ -z "$SERVER_DOMAIN" ]; then
    echo -e "${RED}Ошибка: Переменная SERVER_DOMAIN не найдена в файле $ENV_FILE${NC}"
    echo -e "${YELLOW}Пожалуйста, проверьте файл .env и убедитесь, что он содержит строку: SERVER_DOMAIN=\"docker.local\"${NC}"
    exit 1
fi

echo -e "${GREEN}Найден домен: $SERVER_DOMAIN${NC}"

# Выполнение ping 3 раза
echo -e "${YELLOW}Выполняется ping к $SERVER_DOMAIN (3 попытки)...${NC}"

OS="$(uname -s)"
# Сохраняем вывод ping в переменную
case "$OS" in
    Linux*)
        PING_OUTPUT=$(ping -c 3 "$SERVER_DOMAIN" 2>&1)
        PING_EXIT_CODE=$?
        ;;
    Darwin*)
        PING_OUTPUT=$(ping -c 3 "$SERVER_DOMAIN" 2>&1)
        PING_EXIT_CODE=$?
        ;;
    CYGWIN*|MINGW*|MSYS*)
        PING_OUTPUT=$(ping -n 3 "$SERVER_DOMAIN" 2>&1)
        PING_EXIT_CODE=$?
        ;;
    *)
        PING_OUTPUT="${RED}Ошибка: Неизвестная операционная система. Поддерживаются только Linux, macOS и Windows.${NC}"
        PING_EXIT_CODE=1
        ;;
esac

# Проверка результата ping
if [ $PING_EXIT_CODE -ne 0 ]; then
    echo -e "${RED}Ошибка: Не удалось выполнить ping к $SERVER_DOMAIN${NC}"
    echo -e "${YELLOW}Вывод ошибки:${NC}"
    echo "$PING_OUTPUT"
    echo ""
    echo -e "${YELLOW}Возможные решения:${NC}"
    echo -e "1. Проверьте доступность хоста $SERVER_DOMAIN в вашей сети"
    echo -e "2. Добавьте локальную запись в файл hosts:"
    
    # Определение ОС
    case "$OS" in
        Linux*)
            echo -e "   ${GREEN}Для Linux:${NC} sudo nano /etc/hosts"
            echo -e "   Добавьте строку: 127.0.0.1 $SERVER_DOMAIN"
            ;;
        Darwin*)
            echo -e "   ${GREEN}Для macOS:${NC} sudo nano /etc/hosts"
            echo -e "   Добавьте строку: 127.0.0.1 $SERVER_DOMAIN"
            ;;
        CYGWIN*|MINGW*|MSYS*)
            echo -e "   ${GREEN}Для Windows:${NC} notepad C:\\Windows\\System32\\drivers\\etc\\hosts"
            echo -e "   Добавьте строку: 127.0.0.1 $SERVER_DOMAIN"
            ;;
        *)
            echo -e "   ${GREEN}Неизвестная ОС:${NC} Добавьте запись в файл hosts или его аналог в вашей системе."
            ;;
    esac
    
    exit 1
else
    # Успешный ping
    echo -e "${GREEN}✓ Ping успешно выполнен к $SERVER_DOMAIN${NC}"
    echo -e "${GREEN}Все 3 пакета отправлены и получены успешно${NC}"
    echo ""
    echo -e "${YELLOW}Статистика ping:${NC}"
    # Извлекаем и выводим статистику
    echo "$PING_OUTPUT" | grep -E "(packets transmitted|rtt min/avg/max)" || echo "$PING_OUTPUT" | tail -n 3
    exit 0
fi
