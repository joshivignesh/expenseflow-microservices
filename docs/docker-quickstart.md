# Docker Quick Start

## Prerequisites
- Docker Desktop 4.x+
- `docker-compose` v2+

## 1. Configure secrets

```bash
cp .env.example .env
# Edit .env with real values — never commit this file
```

## 2. Build and start all services

```bash
# First run — builds images and starts containers
docker-compose up --build

# Subsequent runs
docker-compose up -d
```

## 3. Verify services are running

| Service          | URL                          | Description          |
|------------------|------------------------------|----------------------|
| Identity API     | http://localhost:5001        | Swagger UI           |
| Expense API      | http://localhost:5002        | Swagger UI           |
| SQL Server       | localhost:1433               | SA login             |

## 4. Typical dev workflow

```bash
# Register a user
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Vignesh","lastName":"Joshi","email":"v@test.com","password":"Test@1234!"}'

# Login and capture token
TOKEN=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"v@test.com","password":"Test@1234!"}' | jq -r '.accessToken')

# Create an expense
curl -X POST http://localhost:5002/api/expenses \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"description":"Flight to Mumbai","amount":4500,"currency":"INR","category":1,"expenseDate":"2026-03-09T00:00:00Z"}'
```

## 5. Tear down

```bash
# Stop containers (keeps volumes)
docker-compose down

# Stop and delete all data
docker-compose down -v
```

## Service dependency order

```
sqlserver (healthy) → identity-service → expense-service
```

Both API services wait for the SQL Server healthcheck to pass before starting,
then automatically apply EF Core migrations on startup via `DatabaseSeeder`.
