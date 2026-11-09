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

1. Скопируйте `.env.example` в `.env` и заполните (в том числе `ASPNETCORE_ENVIRONMENT=Production`, `FRONTEND_URL`, `PUBLIC_API_URL`, `CORS_ORIGINS`).
2. Запустите:
    ```bash
    docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
    ```
3. Настройте nginx

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

Все значения задаются в `.env` (образец: `.env.example`), docker compose подставляет их в сервисы.

- `ASPNETCORE_ENVIRONMENT` - Development (по умолчанию) или Production
- `FRONTEND_URL`, `PUBLIC_API_URL`, `CORS_ORIGINS` - адреса сайта, API и разрешённые источники
- `AUTH_REQUIRE_CONFIRMED_EMAIL` - вход только после подтверждения почты (по умолчанию true)
- `JWT_KEY`, `INTERNAL_API_KEY`, пароли баз данных и RabbitMQ, `LLM_API_KEY`, SMTP и S3 - секреты

## Конфигурационные файлы

- `docker-compose.yml` - Базовая конфигурация
- `docker-compose.prod.yml` - Настройки для продакшена (сборка сайта, перезапуск, закрытые порты)

## Права доступа

Правила описаны в `Shared/Shared/Access/PracticeAccess.cs` и одинаково применяются в Core и PracticeEntities. Без токена открыты только вход, регистрация, сброс и подтверждение почты и список тем.

| Действие | Кто может |
|---|---|
| Видеть практику и всё, что к ней приложено (отчёты, сообщения, тексты, презентации) | студент, научный руководитель и консультант этой практики; руководитель практики, администратор; рецензент (чтение) |
| Список всех практик | руководитель практики, администратор, рецензент |
| Создать практику | студент для себя; руководитель практики, администратор |
| Менять тему, руководителя, консультанта; удалить практику | студент и научный руководитель этой практики; руководитель практики, администратор |
| Выставить итоговую оценку | научный руководитель этой практики; руководитель практики, администратор |
| Загрузить отзыв | научный руководитель и консультант этой практики; руководитель практики, администратор |
| Добавить отчёт, сообщение, комментарий, цели | любой участник практики и те, кто может её видеть |
| Тема: изменить, архивировать, удалить | автор темы; руководитель практики, администратор |
| Список студентов | руководитель практики, администратор |
| Своя запись студента, преподавателя, консультанта | владелец записи (привязку к пользователю и право руководства менять нельзя); администратор |
| Группы, создание и удаление пользователей и записей, смена ролей | администратор |

Автор сообщений и комментариев берётся из токена, а не из запроса. Сервисы друг другу представляются общим ключом `INTERNAL_API_KEY`.

