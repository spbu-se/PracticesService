# PracticesService

## Описание

**PracticesService** — Сервис для работы с учебными/производственными практиками кафедры. Запуск производится через Docker Compose.

## Архитектура

Сервис состоит из следующих компонентов:

### Backend (микросервисы)
- **gateway.api** (порт 5000) - API Gateway на YARP, агрегирует все сервисы
- **core.api** - Основной сервис практик (PostgreSQL)
- **auth.api** - Сервис авторизации и аутентификации (PostgreSQL)
- **practice-entities.api** - Сервис сущностей практик (MongoDB)
- **rabbitmq** - Брокер сообщений для межсервисной коммуникации

### Базы данных
- **core.db** - PostgreSQL для Core Service
- **auth.db** - PostgreSQL для Auth Service
- **practice-entities.db** - MongoDB для Practice Entities

### Frontend
- **frontend** (порт 8000) - React приложение на Vite

## Предварительные требования

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)

## Запуск проекта

### Разработка

1. Клонируйте репозиторий:
    ```bash
    git clone <URL вашего репозитория>
    cd PracticesService
    ```

2. Запустите все сервисы для разработки:
    ```bash
    docker-compose up --build
    ```

3. Проверьте состояние всех сервисов:
    ```bash
    docker-compose ps
    ```

### Production

1. Соберите и запустите в production режиме:
    ```bash
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
    ```

2. Проверьте логи:
    ```bash
    docker-compose logs -f
    ```

## Доступ к сервисам (разработка)

| Сервис                 | URL                                      | Описание                    |
|------------------------|------------------------------------------|-----------------------------|
| **Frontend**           | http://localhost:8000                    | Клиентское приложение       |
| **Gateway API**        | http://localhost:5000                    | Единая точка входа API      |
| **Gateway Swagger**    | http://localhost:5000/swagger            | Документация Gateway        |
| **Core API Swagger**   | http://localhost:5000/swagger/core.json  | Core Service API            |
| **Auth API Swagger**   | http://localhost:5000/swagger/auth.json  | Auth Service API            |
| **RabbitMQ UI**        | http://localhost:15672                   | Управление RabbitMQ         |
| **PostgreSQL (Core)**  | localhost:5432 (внутри Docker)           | База Core Service           |
| **PostgreSQL (Auth)**  | localhost:5432 (внутри Docker)           | База Auth Service           |
| **MongoDB**            | localhost:27017 (внутри Docker)          | База Practice Entities      |

## Структура API через Gateway

Все запросы проходят через Gateway:
- /core-api/{endpoint} → Core Service
- /auth-api/{endpoint} → Auth Service
- /practice-entities-api/{endpoint} → Practice Entities Service


## Переменные окружения

### Frontend
- `VITE_API_BASE_URL` - Базовый URL API (по умолчанию: http://localhost:5000)

### Backend
- `ASPNETCORE_ENVIRONMENT` - Development/Production
- `RabbitMQ__Host`, `RabbitMQ__Username`, `RabbitMQ__Password`
- `RUN_MIGRATIONS` - Применять миграции при запуске (только Auth)

## Конфигурационные файлы

- `docker-compose.yml` - Базовая конфигурация
- `docker-compose.prod.yml` - Настройки для продакшена