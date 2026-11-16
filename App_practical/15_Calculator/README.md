# 15_Calculator — Jenkins practical work

Минимальное ASP.NET Core MVC приложение (.NET 8) на основе первой практической работы. Поддерживает сложение, вычитание, умножение и деление. MariaDB, Entity Framework Core и Kafka отсутствуют.

## Структура

- `Jenkinsfile` — Pipeline Jenkins.
- `docker-compose.yml` — конфигурация одного веб-контейнера.
- `15_Calculator/` — проект ASP.NET Core MVC и Dockerfile.

## Локальный запуск

Перейдите в `15_Calculator/` и выполните `dotnet restore`, затем `dotnet run`.

## Docker Compose

Запускайте команды из папки, содержащей `docker-compose.yml`: `docker compose up -d --build`. Приложение слушает порт 5015 внутри контейнера и публикуется на порту 5215 хоста. Остановка: `docker compose down`.

## Jenkins

В репозитории, где этот каталог размещён как `App_practical/15_Calculator`, Script Path укажите `App_practical/15_Calculator/Jenkinsfile`. В Pipeline используется существующий агент Jenkins; Jenkins отдельно не разворачивается. Агент должен уже иметь доступ к Docker и Docker Compose.
