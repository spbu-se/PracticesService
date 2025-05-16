# PracticesService

## Описание

**PracticesService** — Сервис для работы с учебными/производственными практиками кафедры. Запуск производится через Docker Compose. 

## Контейнеры

- **RabbitMQ** — брокер сообщений
- **gateway.api** — шлюзовый API
- **core.api / core.db** — основной сервис и его база данных
- **auth.api / auth.db** — сервис авторизации и его база данных
- **frontend** — клиентская часть

## Предварительные требования

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)

## Запуск проекта

1. Клонируйте репозиторий:

    ```bash
    git clone <URL вашего репозитория>
    cd <название директории>
    ```

2. Запустите все сервисы:

    ```bash
    docker-compose up --build
    ```

    Все сервисы поднимутся автоматически, включая RabbitMQ, базы данных и API.

3. Убедитесь, что RabbitMQ доступен по адресу:
    ```
    http://localhost:15672
    ```
    Логин: `admin`, пароль: `admin123`

## Применение миграций для AuthService

После запуска контейнеров, необходимо применить миграции к базе данных авторизации.

1. Войдите в контейнер `auth.api`:

    ```bash
    docker exec -it <CONTAINER_ID_ИЛИ_NAME> /bin/sh
    ```

    Например:

    ```bash
    docker exec -it auth.api /bin/sh
    ```

2. Примените миграции (предположим, используется Entity Framework CLI):

    ```bash
    dotnet ef database update --project Services/AuthService.Api
    ```

    > Убедитесь, что внутри контейнера установлен `dotnet-ef`. Если нет — установите или примените миграции локально и пересоберите образ.

## Доступ к сервисам

| Сервис       | URL                        |
|--------------|----------------------------|
| Gateway API  | http://localhost:5000      |
| Core API     | http://localhost:5001      |
| Auth API     | http://localhost:5002      |
| Frontend     | http://localhost:8000      |
| RabbitMQ UI  | http://localhost:15672     |

## Сеть

Все сервисы находятся в общей Docker-сети `proxybackend`.

---

## Полезные команды

- Остановка всех сервисов:

    ```bash
    docker-compose down
    ```

- Просмотр логов:

    ```bash
    docker-compose logs -f
    ```

- Проверка состояния контейнеров:

    ```bash
    docker ps
    ```



