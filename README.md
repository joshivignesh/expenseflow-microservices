<div align="center">

<br/>

```
███████╗██╗  ██╗██████╗ ███████╗███╗   ██╗███████╗███████╗███████╗██╗      ██████╗ ██╗    ██╗
██╔════╝╚██╗██╔╝██╔══██╗██╔════╝████╗  ██║██╔════╝██╔════╝██╔════╝██║     ██╔═══██╗██║    ██║
█████╗   ╚███╔╝ ██████╔╝█████╗  ██╔██╗ ██║███████╗█████╗  █████╗  ██║     ██║   ██║██║ █╗ ██║
██╔══╝   ██╔██╗ ██╔═══╝ ██╔══╝  ██║╚██╗██║╚════██║██╔══╝  ██╔══╝  ██║     ██║   ██║██║███╗██║
███████╗██╔╝ ██╗██║     ███████╗██║ ╚████║███████║███████╗██║     ███████╗╚██████╔╝╚███╔███╔╝
╚══════╝╚═╝  ╚═╝╚═╝     ╚══════╝╚═╝  ╚═══╝╚══════╝╚══════╝╚═╝     ╚══════╝ ╚═════╝  ╚══╝╚══╝
```

### Enterprise Expense Management — Microservices Architecture

<br/>

[![Build & Test](https://github.com/joshivignesh/expenseflow-microservices/actions/workflows/ci.yml/badge.svg)](https://github.com/joshivignesh/expenseflow-microservices/actions/workflows/ci.yml)
[![Deploy](https://github.com/joshivignesh/expenseflow-microservices/actions/workflows/cd.yml/badge.svg)](https://github.com/joshivignesh/expenseflow-microservices/actions/workflows/cd.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=black)](https://react.dev)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Event%20Bus-FF6600?logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-22c55e.svg)](LICENSE)

<br/>

**[🚀 Live Demo](https://expenseflow.vercel.app)** · **[📖 API Docs](https://expenseflow-gateway.onrender.com/swagger)** · **[🏗️ Architecture](docs/architecture.md)** · **[📋 ADRs](docs/adr/)**

<br/>

</div>

---

## 📌 What Is This?

**ExpenseFlow** is a **production-grade, cloud-native expense management platform** built with enterprise .NET patterns — the same architectural approach used at scale by teams at Microsoft, ThoughtWorks, and the financial services industry.

This project demonstrates:

- **Domain-Driven Design (DDD)** — Aggregate Roots, Value Objects, Domain Events, Bounded Contexts
- **CQRS with MediatR** — Commands use EF Core + rich domain; Queries use Dapper for raw SQL performance
- **Event-Driven Microservices** — Asynchronous communication via RabbitMQ Integration Events
- **Clean Architecture** — Strict layer dependency rules enforced by automated architecture tests
- **Cloud-Native Resilience** — Polly retry/circuit breaker, health checks, OpenTelemetry distributed tracing

> 📚 Architecture follows Microsoft's official guides: [Microservices for .NET](https://learn.microsoft.com/dotnet/architecture/microservices/), [Cloud Native .NET](https://learn.microsoft.com/dotnet/architecture/cloud-native/), and [ASP.NET Core Architecture](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/)

---

## 🏗️ System Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                    React 18 + TypeScript Frontend                    │
│               Redux Toolkit · Tailwind CSS · React Query             │
└────────────────────────────┬─────────────────────────────────────────┘
                             │ HTTPS
                             ▼
┌──────────────────────────────────────────────────────────────────────┐
│                    Ocelot API Gateway  :5000                         │
│         JWT Validation · Rate Limiting · Correlation IDs             │
└──────┬───────────────┬──────────────────┬────────────┬──────────────┘
       │               │                  │            │
       ▼               ▼                  ▼            ▼
┌──────────────┐ ┌──────────────┐ ┌────────────┐ ┌────────────────┐
│  Identity    │ │   Expense    │ │Notification│ │    Report      │
│  Service     │ │   Service    │ │  Service   │ │    Service     │
│   :5001      │ │   :5002      │ │   :5003    │ │    :5004       │
│              │ │              │ │            │ │                │
│  DDD + CQRS  │ │  DDD + CQRS  │ │  Consumer  │ │  Read-heavy    │
│  JWT + Auth  │ │  Approvals   │ │  SendGrid  │ │  QuestPDF      │
└──────┬───────┘ └──────┬───────┘ └─────┬──────┘ └────────────────┘
       │                │               ▲
       ▼                ▼               │ Integration Events
  ┌──────────┐    ┌──────────┐    ┌─────┴──────────┐
  │SQL Server│    │SQL Server│    │    RabbitMQ     │
  │(Identity)│    │(Expense) │    │   Event Bus     │
  └──────────┘    └────┬─────┘    └────────────────┘
                       │
                  ┌────▼─────┐
                  │  Redis   │
                  │  Cache   │
                  └──────────┘
```

### Microservices at a Glance

| Service | Pattern | Key Tech | Port |
|---|---|---|---|
| **Identity.Service** | DDD + CQRS | JWT, Refresh Tokens, EF Core | 5001 |
| **Expense.Service** | DDD + CQRS + Events | MediatR, Dapper, RabbitMQ, Redis | 5002 |
| **Notification.Service** | Consumer | RabbitMQ, SendGrid, HTML Templates | 5003 |
| **Report.Service** | Query-only | Dapper, QuestPDF, Analytics | 5004 |
| **API Gateway** | Reverse Proxy | Ocelot, Polly, JWT Validation | 5000 |

---

## 🧠 Architecture Deep Dive

### Domain-Driven Design

Each microservice is its own **Bounded Context** with a fully isolated domain model. The `Expense` aggregate root is the centrepiece:

```csharp
// ✅ Rich domain model — state changes via explicit domain methods
// Source: Microsoft Microservices Guide, Chapter 7
public class Expense : AggregateRoot
{
    public Money Amount { get; private set; }        // Value Object
    public ExpenseStatus Status { get; private set; } // Private setter
    public string SubmittedById { get; private set; }

    // Domain method — enforces business rules + raises Domain Event internally
    public void Approve(string approverId, string comment)
    {
        if (Status != ExpenseStatus.Pending)
            throw new InvalidExpenseStateException(Id, Status, "Approve");

        Status = ExpenseStatus.Approved;
        AddDomainEvent(new ExpenseApprovedEvent(Id, approverId, comment));
    }

    // ❌ No public setters. No: expense.Status = ExpenseStatus.Approved;
}
```

### CQRS — Write Side (Commands via EF Core + Domain)

```csharp
// Command — immutable C# record
public sealed record ApproveExpenseCommand(Guid ExpenseId, string ApproverId, string Comment)
    : IRequest<bool>;

// Handler — orchestrates, delegates logic to domain
public class ApproveExpenseCommandHandler : IRequestHandler<ApproveExpenseCommand, bool>
{
    public async Task<bool> Handle(ApproveExpenseCommand cmd, CancellationToken ct)
    {
        var expense = await _repository.GetByIdAsync(cmd.ExpenseId, ct);
        expense.Approve(cmd.ApproverId, cmd.Comment); // Domain logic lives HERE
        await _repository.UnitOfWork.SaveEntitiesAsync(ct);
        return true;
    }
}
```

### CQRS — Read Side (Queries via Dapper — raw SQL performance)

```csharp
// Query handler bypasses domain entirely — max read performance
// As recommended in Microsoft's eShopOnContainers reference
public class GetExpensesByUserHandler : IRequestHandler<GetExpensesByUserQuery, PagedResult<ExpenseSummaryViewModel>>
{
    public async Task<PagedResult<ExpenseSummaryViewModel>> Handle(GetExpensesByUserQuery query, CancellationToken ct)
    {
        const string sql = @"
            SELECT e.Id, e.Amount_Value AS Amount, e.Currency, e.Category,
                   e.Status, e.SubmittedAt, e.Description
            FROM Expenses e
            WHERE e.SubmittedById = @UserId AND e.Status = @Status
            ORDER BY e.SubmittedAt DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

        var results = await _dbConnection.QueryAsync<ExpenseSummaryViewModel>(sql, query);
        return results.ToPagedResult(query.Page, query.PageSize);
    }
}
```

### MediatR Pipeline Behaviors (Cross-cutting Concerns)

```
HTTP Request
    ↓
Controller.Send(command)
    ↓
[LoggingBehavior]       ← Logs command name, userId, duration
    ↓
[ValidationBehavior]    ← FluentValidation — fails fast before touching domain
    ↓
[TransactionBehavior]   ← Wraps handler in DB transaction
    ↓
CommandHandler          ← Pure business orchestration
    ↓
Domain.Method()         ← Business rules + raises Domain Events
    ↓
Repository.Save()       ← EF Core persists, publishes integration events
```

### Event-Driven Integration

Domain Events stay inside a service. Integration Events cross service boundaries via RabbitMQ:

```
Expense Service                          Notification Service
───────────────                          ────────────────────
expense.Approve()
 └─► ExpenseApprovedDomainEvent (internal)
      └─► CommandHandler publishes ──────────────────────────►  ExpenseApprovedConsumer
           ExpenseApprovedIntegrationEvent                            └─► EmailService.Send()
           to RabbitMQ                                                      └─► SendGrid API
```

### Resilience with Polly

```csharp
// Every outbound HTTP call wrapped in retry + circuit breaker
// Source: Cloud-Native .NET Guide, Chapter 6
services.AddHttpClient<IIdentityClient, IdentityClient>()
    .AddPolicyHandler(GetRetryPolicy())        // Exponential backoff: 1s, 2s, 4s
    .AddPolicyHandler(GetCircuitBreakerPolicy()); // Open circuit after 5 failures, reset after 30s
```

### Health Checks (Kubernetes-ready)

Every service exposes two endpoints:

| Endpoint | Purpose | Checks |
|---|---|---|
| `GET /health/live` | Liveness — is the process alive? | Always 200 OK |
| `GET /health/ready` | Readiness — can it take traffic? | DB connectivity + Redis ping |

---

## 📁 Repository Structure

```
expenseflow-microservices/
├── src/
│   ├── services/
│   │   ├── ExpenseFlow.Identity/          # 4-layer DDD: Domain/Application/Infrastructure/API
│   │   ├── ExpenseFlow.Expense/           # Core bounded context — richest domain model
│   │   ├── ExpenseFlow.Notification/      # Event consumer — simple, no domain layer needed
│   │   └── ExpenseFlow.Report/            # Read-only service — Dapper + QuestPDF
│   ├── gateway/
│   │   └── ExpenseFlow.Gateway/           # Ocelot routing, JWT validation, rate limiting
│   └── frontend/
│       └── expenseflow-ui/                # React 18 + TypeScript + Tailwind + Redux Toolkit
├── shared/
│   └── ExpenseFlow.Shared/                # AggregateRoot, Entity, ValueObject base classes
├── infrastructure/
│   ├── docker-compose.yml
│   └── nginx/nginx.conf
├── tests/
│   ├── ExpenseFlow.Expense.UnitTests/     # Domain logic — no mocking of infrastructure
│   ├── ExpenseFlow.Identity.UnitTests/
│   ├── ExpenseFlow.Expense.IntegrationTests/  # Real DB via TestContainers
│   └── ExpenseFlow.Architecture.Tests/   # ArchUnitNET — enforces layer rules in CI
├── docs/
│   ├── architecture.md
│   └── adr/                              # Architecture Decision Records
├── .github/workflows/
│   ├── ci.yml                            # Build → Test → Lint → Arch tests on every PR
│   └── cd.yml                            # Docker build → push → deploy on merge to main
└── README.md
```

---

## 🚀 Running Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js 20+](https://nodejs.org)

### One-Command Startup

```bash
git clone https://github.com/joshivignesh/expenseflow-microservices.git
cd expenseflow-microservices
docker-compose up --build
```

That's it. Docker Compose starts all services, databases, Redis, and RabbitMQ.

| URL | What |
|---|---|
| `http://localhost:3000` | React Frontend |
| `http://localhost:5000` | API Gateway (Swagger UI) |
| `http://localhost:15672` | RabbitMQ Management (guest/guest) |
| `http://localhost:5001/health/ready` | Identity Service Health |
| `http://localhost:5002/health/ready` | Expense Service Health |

### Seeded Test Accounts

| Role | Email | Password |
|---|---|---|
| Admin | `admin@expenseflow.com` | `Admin@123` |
| Employee | `employee@expenseflow.com` | `Pass@123` |
| Manager | `manager@expenseflow.com` | `Pass@123` |

---

## 🧪 Running Tests

```bash
# All tests
dotnet test

# Unit tests only (fast — no external dependencies)
dotnet test --filter Category=Unit

# Integration tests (requires Docker for TestContainers)
dotnet test --filter Category=Integration

# Architecture tests — enforces DDD layer rules
dotnet test tests/ExpenseFlow.Architecture.Tests/
```

**Architecture test example** — this runs in CI and will fail if anyone imports EF Core into the Domain layer:

```csharp
[Fact]
public void DomainLayer_Should_Not_Depend_On_Infrastructure()
{
    var result = Types.InAssembly(DomainAssembly)
        .ShouldNot()
        .HaveDependencyOn("Microsoft.EntityFrameworkCore")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

---

## 🌐 Deployment

All services deployed for free using:

| Component | Platform | Status |
|---|---|---|
| API Gateway + Microservices | Render.com | [![Render](https://img.shields.io/badge/Render-deployed-46E3B7?logo=render)](https://render.com) |
| React Frontend | Vercel | [![Vercel](https://img.shields.io/badge/Vercel-deployed-000000?logo=vercel)](https://vercel.com) |
| PostgreSQL | Railway.app | Running |
| Redis | Upstash | Running |
| RabbitMQ | CloudAMQP | Running |

**CI/CD Pipeline:**

```
Push to feature/* → CI runs (build + test + arch checks)
                          ↓
Merge to develop → Integration tests
                          ↓
Merge to main   → Docker build → Docker Hub push → Render deploy
```

---

## 📐 Key Design Decisions

See [Architecture Decision Records](docs/adr/) for full context. Summary:

| Decision | Choice | Why |
|---|---|---|
| CQRS read strategy | **Dapper** (not EF) | Raw SQL performance; no domain constraints on reads |
| Messaging | **RabbitMQ** (not HTTP) | Services stay decoupled; failures don't cascade |
| Validation | **MediatR Pipeline** (not controller) | Centralized — works for any entry point |
| DDD scope | **Expense + Identity only** | Notification/Report are simple — DDD overkill there |
| Integration testing | **TestContainers** | Tests run against real DB — not mocks |
| Layer enforcement | **ArchUnitNET in CI** | Rules that aren't automated get broken eventually |

---

## 🛠️ Tech Stack

<table>
<tr>
<td><strong>Backend</strong></td>
<td>ASP.NET Core 8 · C# 12 · Entity Framework Core 8 · Dapper</td>
</tr>
<tr>
<td><strong>Architecture</strong></td>
<td>DDD · CQRS · Clean Architecture · Event-Driven</td>
</tr>
<tr>
<td><strong>Messaging</strong></td>
<td>RabbitMQ via MassTransit</td>
</tr>
<tr>
<td><strong>Caching</strong></td>
<td>Redis via StackExchange.Redis</td>
</tr>
<tr>
<td><strong>Resilience</strong></td>
<td>Polly (Retry + Circuit Breaker) · IHttpClientFactory</td>
</tr>
<tr>
<td><strong>Observability</strong></td>
<td>Serilog (structured JSON) · OpenTelemetry · Health Checks</td>
</tr>
<tr>
<td><strong>Auth</strong></td>
<td>JWT Bearer + Refresh Token Rotation</td>
</tr>
<tr>
<td><strong>Gateway</strong></td>
<td>Ocelot (routing, rate limiting, auth)</td>
</tr>
<tr>
<td><strong>Frontend</strong></td>
<td>React 18 · TypeScript · Redux Toolkit · Tailwind CSS · Recharts</td>
</tr>
<tr>
<td><strong>Testing</strong></td>
<td>xUnit · Moq · FluentAssertions · TestContainers · ArchUnitNET</td>
</tr>
<tr>
<td><strong>DevOps</strong></td>
<td>Docker · Docker Compose · GitHub Actions · Render · Vercel</td>
</tr>
<tr>
<td><strong>Reports</strong></td>
<td>QuestPDF · Dapper analytics queries</td>
</tr>
</table>

---

## 📊 Features

- **Submit expenses** with line items, categories, receipt URLs, and period ranges
- **Approval workflow** — Managers approve or reject with comments; employees notified via email
- **Dashboard** — Expense totals, category breakdown chart, pending approvals count
- **PDF Reports** — Monthly expense summaries downloadable as formatted PDF
- **Admin panel** — Full expense management, user management, audit trails
- **Role-based access** — Employee, Manager, Admin roles enforced at gateway + service level
- **Pagination & filtering** — Filter by status, date range, category, amount
- **Correlation tracking** — Every request traceable across all services via `X-Correlation-Id`

---

## 👤 Author

**Vignesh Joshi** — .NET Full Stack Developer

[![LinkedIn](https://img.shields.io/badge/LinkedIn-joshivignesh-0077B5?logo=linkedin)](https://linkedin.com/in/joshivignesh)
[![GitHub](https://img.shields.io/badge/GitHub-joshivignesh-181717?logo=github)](https://github.com/joshivignesh)
[![Twitter](https://img.shields.io/badge/Twitter-@vigneshjoshi-1DA1F2?logo=twitter)](https://twitter.com/vigneshjoshi)

---

## 📄 License

MIT — see [LICENSE](LICENSE)

---

<div align="center">

*Built with the patterns from Microsoft's .NET Architecture Guides*
*eShopOnContainers · Domain-Driven Design · Cloud-Native .NET*

</div>
