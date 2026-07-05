# PracticesService

[![CI](https://github.com/spbu-se/PracticesService/actions/workflows/main.yml/badge.svg)](https://github.com/spbu-se/PracticesService/actions/workflows/main.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Сервис для работы с учебными и производственными практиками кафедры. Запуск производится через Docker Compose.

## Описание

PracticesService — микросервисная платформа для управления процессом прохождения учебных практик.

## Архитектура

### Backend (микросервисы)

| Сервис | Порт | Описание |
|--------|------|----------|
| gateway.api | 5000 | API Gateway на YARP |
| core.api | 8080 | Основной сервис практик (PostgreSQL) |
| auth.api | 8080 | Сервис авторизации (PostgreSQL) |
| practice-entities.api | 8080 | Сервис сущностей практик (MongoDB) |
| notification.api | 8080 | Сервис уведомлений (SMTP, RabbitMQ) |
| audit.api | 8080 | Сервис аудита |
| llm.api | 8080 | Сервис анализа документов |
| rabbitmq | 15672 | Брокер сообщений |

### Базы данных

| Сервис | БД | Назначение |
|--------|-----|------------|
| core.db | PostgreSQL | Практики, темы, студенты, преподаватели |
| auth.db | PostgreSQL | Пользователи и роли |
| practice-entities.db | MongoDB | Документы, фидбеки, сообщения |

### Frontend

| Сервис | Порт | Описание |
|--------|------|----------|
| frontend | 8000 | React приложение на Vite |

## Запуск проекта

### Предварительные требования

- Docker
- Docker Compose

### Разработка

```bash
git clone <URL репозитория>
cd PracticesService
docker-compose up --build
```

### Проверка состояния

```bash
docker-compose ps
docker-compose logs -f
```

### Production

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

## Доступ к сервисам

| Сервис | URL |
|--------|-----|
| Frontend | http://localhost:8000 |
| Gateway API | http://localhost:5000 |
| Gateway Swagger | http://localhost:5000/swagger |
| Core API Swagger | http://localhost:5000/core-swagger |
| Auth API Swagger | http://localhost:5000/auth-swagger |
| Practice Entities Swagger | http://localhost:5000/practice-entities-swagger |
| Notification Swagger | http://localhost:5000/notification-swagger |
| Audit Swagger | http://localhost:5000/audit-swagger |
| LLM Swagger | http://localhost:5000/llm-swagger |
| RabbitMQ UI | http://localhost:15672 |

## API Маршруты через Gateway

| Префикс | Сервис |
|---------|--------|
| /core-api/{**catch-all} | Core Service |
| /auth-api/{**catch-all} | Auth Service |
| /practice-entities-api/{**catch-all} | Practice Entities Service |
| /notification-api/{**catch-all} | Notification Service |
| /audit-api/{**catch-all} | Audit Service |
| /llm-api/{**catch-all} | LLM Service |

## Переменные окружения

### Frontend

| Переменная | Описание | Значение по умолчанию |
|------------|----------|----------------------|
| VITE_API_BASE_URL | Базовый URL API | http://localhost:5000 |

### Backend

| Переменная | Описание |
|------------|----------|
| ASPNETCORE_ENVIRONMENT | Development/Production |
| RabbitMQ__Host | Хост RabbitMQ |
| RabbitMQ__Username | Имя пользователя RabbitMQ |
| RabbitMQ__Password | Пароль RabbitMQ |
| RUN_MIGRATIONS | Применять миграции при запуске |

## Конфигурационные файлы

| Файл | Назначение |
|------|------------|
| docker-compose.yml | Базовая конфигурация |
| docker-compose.prod.yml | Настройки для продакшена |


## Лицензия

MIT
