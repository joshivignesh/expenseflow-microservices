# ExpenseFlow Microservices

> A production-grade expense management platform built with **.NET 8 microservices**, **React**, **Docker**, and **Clean Architecture** — developed over 18 days as a portfolio project demonstrating enterprise-level backend engineering.

[![Build](https://github.com/joshivignesh/expenseflow-microservices/actions/workflows/ci.yml/badge.svg)](https://github.com/joshivignesh/expenseflow-microservices/actions)
[![Integration Tests](https://github.com/joshivignesh/expenseflow-microservices/actions/workflows/integration-tests.yml/badge.svg)](https://github.com/joshivignesh/expenseflow-microservices/actions)

---

## Architecture Overview

```
┌──────────────────────────────────┐
│  React Frontend (Vite + TS)  │  :3000
└───────────┬──────────────────────┘
           │ Nginx proxy
┌───────────┴──────────────────────┐
│   YARP API Gateway           │  :5000
│   Rate limiting • JWT • CORS │
└───────┬───────────────┬─────┘
        │               │
┌───────┴────┐ ┌───────┴────┐
│ Identity API  │ │ Expense API  │
│ :5001         │ │ :5002        │
└───────┬────┘ └───────┬────┘
        │               │
┌───────┴───────────────┴────┐
│   SQL Server 2022          │
│   IdentityDb + ExpenseDb   │
└──────────────────────────┘
```

## Key Design Decisions

| Concern | Choice | Why |
|---|---|---|
| Architecture | Clean Architecture + DDD | Enforces dependency rule; domain has zero external deps |
| Messaging | MediatR (CQRS) | Commands and queries separated; thin controllers |
| ORM (writes) | EF Core 8 | Strong typing, migrations, owned types for value objects |
| ORM (reads) | Dapper | Raw SQL for query-side — no change tracking overhead |
| Auth | JWT HS256 | Stateless; shared secret validates tokens cross-service |
| Gateway | YARP | Config-driven reverse proxy; zero custom routing code |
| Tests | Testcontainers + WebApplicationFactory | Real SQL, real pipeline — no mocks |
| Observability | OpenTelemetry + Serilog + Prometheus | Vendor-neutral; works locally and in Azure |

## Quick Start

```bash
# 1. Clone
git clone https://github.com/joshivignesh/expenseflow-microservices.git
cd expenseflow-microservices

# 2. Configure secrets
cp .env.example .env
# Edit .env: set SQL_SA_PASSWORD and JWT_SECRET_KEY

# 3. Start the full stack
docker-compose up --build

# 4. Open the app
open http://localhost:3000
```

| URL | What |
|---|---|
| http://localhost:3000 | React Frontend |
| http://localhost:5000 | API Gateway |
| http://localhost:5001 | Identity Service Swagger |
| http://localhost:5002 | Expense Service Swagger |
| http://localhost:16686 | Jaeger Traces (with observability stack) |
| http://localhost:9090 | Prometheus Metrics |
| http://localhost:3001 | Grafana Dashboards |

## Repository Structure

```
expenseflow-microservices/
├── shared/                          # Shared building blocks (DDD base classes)
├── src/
│   ├── services/
│   │   ├── ExpenseFlow.Identity/    # Authentication & user management
│   │   └── ExpenseFlow.Expense/     # Expense lifecycle management
│   ├── gateway/
│   │   └── ExpenseFlow.Gateway/     # YARP reverse proxy
│   └── frontend/                    # React + Vite + TypeScript
├── tests/
│   ├── ExpenseFlow.Identity.IntegrationTests/
│   └── ExpenseFlow.Expense.IntegrationTests/
├── docs/                            # ADRs and architecture docs
├── .github/workflows/               # CI, integration tests, deploy
├── docker-compose.yml
├── docker-compose.override.yml      # Dev overrides
└── docker-compose.observability.yml # Optional Jaeger+Prometheus+Grafana
```

## 18-Day Build Log

| Day | What was built |
|---|---|
| 1–3 | Repo setup, README, ADRs, GitHub Actions CI |
| 4–6 | Identity Domain, Application commands/queries, MediatR behaviors |
| 7–9 | Identity Infrastructure (EF Core, JWT, PBKDF2) + API layer |
| 10–11 | Expense Domain (state machine, Money VO) + Infrastructure |
| 12–13 | Expense Application (CQRS) + API layer |
| 14 | Docker multi-stage builds + docker-compose |
| 15 | YARP API Gateway — routing, rate limiting, CORS |
| 16 | Integration tests — Testcontainers + WebApplicationFactory |
| 17 | React frontend — Vite + TanStack Query + Zustand |
| 18 | Observability (OTel/Serilog/Prometheus) + Azure CD pipeline |

## AI Model Assignment

This project uses Anthropic models for AI-assisted features:

| Model | Role | Use case |
|---|---|---|
| `claude-sonnet-4-6` | Brain / Routing | Orchestration, decisions, complex reasoning |
| `claude-haiku-4-5-20251001` | Heartbeat | Health checks, lightweight classification |
| `claude-opus-4-6` | Heavy lifting | Complex analysis, document processing |

## Author

**Vignesh Joshi** — .NET Full Stack Developer, PwC Chennai
