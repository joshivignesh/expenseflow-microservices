# Local Development Setup Guide

## Prerequisites

| Tool | Version | Download |
|---|---|---|
| .NET SDK | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Docker Desktop | Latest | [docker.com](https://www.docker.com/products/docker-desktop) |
| Node.js | 20+ | [nodejs.org](https://nodejs.org) |
| Git | Any | [git-scm.com](https://git-scm.com) |

---

## Option 1 — Full Stack via Docker Compose (Recommended)

This starts everything: all 4 microservices, API gateway, SQL Server, Redis, RabbitMQ, and the React frontend.

```bash
# 1. Clone the repo
git clone https://github.com/joshivignesh/expenseflow-microservices.git
cd expenseflow-microservices

# 2. Start everything
docker-compose up --build

# 3. Wait ~60 seconds for all services to initialise
# Then open:
#   Frontend:  http://localhost:3000
#   Gateway:   http://localhost:5000/swagger
#   RabbitMQ:  http://localhost:15672  (guest / guest)
```

To stop:
```bash
docker-compose down

# To also remove volumes (wipes DB data):
docker-compose down -v
```

---

## Option 2 — Run Individual Services

Useful when working on a specific service.

```bash
# Start only infrastructure (DB, Redis, RabbitMQ)
docker-compose up sqlserver redis rabbitmq

# Run Identity service
cd src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.API
dotnet run

# Run Expense service (new terminal)
cd src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.API
dotnet run

# Run Frontend (new terminal)
cd src/frontend/expenseflow-ui
npm install
npm run dev
```

---

## Environment Variables

For local development, `appsettings.Development.json` uses localhost defaults.

For production / CI, set these as environment variables or GitHub Secrets:

```
JWT_SECRET_KEY              # Min 32 characters
DB_IDENTITY_CONNECTION      # SQL Server connection string
DB_EXPENSE_CONNECTION       # SQL Server connection string  
REDIS_CONNECTION            # Redis connection string
RABBITMQ_CONNECTION         # RabbitMQ connection string
SENDGRID_API_KEY            # For email notifications
```

> ⚠️ Never commit secrets to the repository. Use GitHub Actions secrets for CI/CD.

---

## Running Database Migrations

```bash
# Identity service
cd src/services/ExpenseFlow.Identity/ExpenseFlow.Identity.Infrastructure
dotnet ef database update --startup-project ../ExpenseFlow.Identity.API

# Expense service
cd src/services/ExpenseFlow.Expense/ExpenseFlow.Expense.Infrastructure
dotnet ef database update --startup-project ../ExpenseFlow.Expense.API
```

Or use Docker Compose — migrations run automatically on startup via `DatabaseSeeder`.

---

## Running Tests

```bash
# All tests
dotnet test

# Specific test project
dotnet test tests/ExpenseFlow.Expense.UnitTests/

# With coverage report
dotnet test --collect:"XPlat Code Coverage"

# Architecture tests (enforces DDD layer rules)
dotnet test tests/ExpenseFlow.Architecture.Tests/
```

> Integration tests require Docker running — they use TestContainers to spin up a real SQL Server instance.

---

## Seeded Accounts

The database seeder creates these accounts on first run:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@expenseflow.com` | `Admin@123` |
| Manager | `manager@expenseflow.com` | `Pass@123` |
| Employee | `employee@expenseflow.com` | `Pass@123` |

---

## Health Check Endpoints

Verify each service is running:

```bash
curl http://localhost:5001/health/ready   # Identity
curl http://localhost:5002/health/ready   # Expense
curl http://localhost:5003/health/ready   # Notification
curl http://localhost:5004/health/ready   # Report
```

Expected response:
```json
{ "status": "Healthy", "checks": { "database": "Healthy", "redis": "Healthy" } }
```

---

## Useful Docker Commands

```bash
# View running containers
docker ps

# View logs for a specific service
docker-compose logs -f expense-service

# Rebuild a single service after code changes
docker-compose up --build expense-service

# Shell into a running container
docker exec -it expenseflow-expense-api bash
```
