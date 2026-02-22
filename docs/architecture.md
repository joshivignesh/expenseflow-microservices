# ExpenseFlow — System Architecture

> Architecture grounded in Microsoft's official .NET guides: [Microservices for .NET](https://learn.microsoft.com/dotnet/architecture/microservices/), [Cloud-Native .NET](https://learn.microsoft.com/dotnet/architecture/cloud-native/), [ASP.NET Core Architecture](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/)

---

## 1. Architectural Style

ExpenseFlow uses a **Microservices Architecture** where each service:
- Owns its own **database** (database-per-service pattern)
- Communicates **asynchronously** via RabbitMQ for state-changing events
- Is independently **deployable** via Docker
- Has its own **Bounded Context** in DDD terms

---

## 2. Bounded Contexts

Each microservice represents one Bounded Context — a term from Domain-Driven Design meaning a self-contained domain with its own language, models, and rules.

```
┌─────────────────────┐   ┌─────────────────────┐
│   Identity Context  │   │   Expense Context   │
│                     │   │                     │
│  User               │   │  Expense (AR*)      │
│  RefreshToken       │   │  ExpenseLineItem    │
│  Email (VO*)        │   │  Receipt            │
│                     │   │  Money (VO*)        │
│  Commands:          │   │  ExpensePeriod (VO*)│
│  - RegisterUser     │   │                     │
│  - LoginUser        │   │  Commands:          │
│                     │   │  - SubmitExpense    │
│  Queries:           │   │  - ApproveExpense   │
│  - GetUserProfile   │   │  - RejectExpense    │
└─────────────────────┘   │                     │
                          │  Queries:           │
┌─────────────────────┐   │  - GetExpenseById   │
│Notification Context │   │  - GetExpensesByUser│
│                     │   └─────────────────────┘
│  (No domain model)  │
│  Consumers:         │   ┌─────────────────────┐
│  - OnExpenseSubmit  │   │   Report Context    │
│  - OnExpenseApprove │   │                     │
│                     │   │  (No domain model)  │
│  Services:          │   │  Queries only:      │
│  - EmailService     │   │  - MonthlyReport    │
└─────────────────────┘   │  - CategoryBreakdown│
                          │                     │
                          │  Services:          │
                          │  - PdfReportService │
                          └─────────────────────┘
```

> *AR = Aggregate Root, VO = Value Object*

**Key DDD rule applied:** Notification and Report services have NO domain layer — they are simple enough to use direct queries. DDD should only be applied where business complexity warrants it (Microsoft Microservices Guide, Ch. 7).

---

## 3. Internal Layer Architecture (per DDD service)

For Identity and Expense services, we apply **Clean Architecture** with 4 layers:

```
┌──────────────────────────────────┐
│           API Layer              │  ← HTTP Controllers, Middleware, Health Checks
│  Depends on: Application         │    No business logic here
└──────────────┬───────────────────┘
               │
┌──────────────▼───────────────────┐
│       Application Layer          │  ← Commands, Queries, Handlers, Behaviors
│  Depends on: Domain              │    Orchestrates — does NOT contain business rules
└──────────────┬───────────────────┘
               │
┌──────────────▼───────────────────┐  ← Aggregate Roots, Entities, Value Objects
│         Domain Layer  ⭐          │    Domain Events, Domain Services, Interfaces
│  Depends on: NOTHING             │    Zero framework dependencies
└──────────────────────────────────┘
               ↑
┌──────────────┴───────────────────┐
│      Infrastructure Layer        │  ← EF Core, Dapper, Redis, RabbitMQ, SendGrid
│  Depends on: Domain              │    Implements interfaces defined in Domain
└──────────────────────────────────┘
```

**The Dependency Rule:** Source code dependencies point inward. The Domain layer has zero dependencies on any other layer or framework. This is enforced by ArchUnitNET tests in CI.

---

## 4. CQRS Pattern

We use **Simplified CQRS** (single database, two logical models) as recommended by Microsoft's eShopOnContainers reference:

```
                    ┌─────────────────┐
                    │  API Controller │
                    └────────┬────────┘
                             │ MediatR.Send()
               ┌─────────────┴─────────────┐
               │                           │
    ┌──────────▼──────────┐   ┌────────────▼────────────┐
    │    WRITE SIDE        │   │      READ SIDE           │
    │    (Commands)        │   │      (Queries)           │
    │                      │   │                         │
    │  Immutable records   │   │  Bypass domain entirely │
    │  Domain model        │   │  Dapper + raw SQL       │
    │  EF Core (write)     │   │  Flat ViewModels/DTOs   │
    │  Repository pattern  │   │  Max read performance   │
    │  Domain events       │   │  No aggregate rules     │
    └──────────┬──────────┘   └────────────┬────────────┘
               │                           │
               └─────────────┬─────────────┘
                             │
                    ┌────────▼────────┐
                    │   SQL Database  │
                    └─────────────────┘
```

**Why Dapper for reads?** From Microsoft's guide: *"The simplest approach for queries in a CQRS microservice can be implemented by querying the database with a Micro-ORM like Dapper, returning dynamic ViewModels. Queries are idempotent — they won't change data no matter how many times you run them."*

---

## 5. MediatR Pipeline

Every command flows through a pipeline of behaviors before reaching the handler:

```
HTTP POST /api/expenses
    │
    ▼
ExpenseController.SubmitExpense()
    │ _mediator.Send(command)
    ▼
┌─────────────────────────────────┐
│      MediatR Pipeline           │
│                                 │
│  1. LoggingBehavior             │ ← Logs: command name, user, timestamp
│  2. ValidationBehavior          │ ← FluentValidation: fails fast if invalid
│  3. TransactionBehavior         │ ← Begins DB transaction
│  4. SubmitExpenseCommandHandler │ ← Your actual handler code
│                                 │
└─────────────────────────────────┘
    │
    ▼
Expense.Submit()    ← Domain method — business rules enforced here
    │
    ▼
Repository.Save()   ← EF Core + publishes integration event to RabbitMQ
```

Pipeline behaviors are registered once and apply automatically to all commands — no repetition in handlers.

---

## 6. Event Architecture

Two types of events — critically different:

### Domain Events (internal to a service)
```csharp
// Raised inside the aggregate, handled within the same transaction
public class ExpenseApprovedEvent : IDomainEvent
{
    public Guid ExpenseId { get; }
    public string ApproverId { get; }
    public DateTime ApprovedAt { get; }
}
```

### Integration Events (cross-service via RabbitMQ)
```csharp
// Published AFTER transaction commits — crosses service boundary
public class ExpenseApprovedIntegrationEvent : IIntegrationEvent
{
    public Guid ExpenseId { get; }
    public string EmployeeEmail { get; }
    public decimal Amount { get; }
    public string ApproverName { get; }
}
```

### Flow
```
Expense Service (Publisher)           Notification Service (Consumer)
──────────────────────────           ───────────────────────────────

expense.Approve()                     
  └─ raises DomainEvent               
       └─ handler publishes  ──────►  ExpenseApprovedConsumer.Consume()
            IntegrationEvent               └─ EmailService.SendApprovalEmail()
            to RabbitMQ                          └─ SendGrid API
```

**Why not direct HTTP calls between services?** If Notification service is down, the Expense service would fail too. With RabbitMQ, the message is queued and delivered when Notification recovers — services are truly decoupled.

---

## 7. Resilience Patterns (Polly)

From Microsoft's Cloud-Native guide, all outbound HTTP calls use:

| Pattern | Config | Behaviour |
|---|---|---|
| **Retry** | 3 attempts, exponential backoff (1s, 2s, 4s) | Retries on transient network errors |
| **Circuit Breaker** | Opens after 5 consecutive failures, resets after 30s | Stops hammering a failing service |
| **Timeout** | 10s per request | Prevents indefinite hangs |

```csharp
services.AddHttpClient<IIdentityClient, IdentityClient>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))))
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

---

## 8. Observability

From Microsoft's Cloud-Native guide: *"Observability is a key design consideration. Centralized logging, monitoring, and alerts become mandatory in a distributed system."*

| Pillar | Tool | What It Captures |
|---|---|---|
| **Structured Logging** | Serilog | JSON logs with correlation ID, user, service name |
| **Distributed Tracing** | OpenTelemetry | End-to-end request trace across all 4 services |
| **Metrics** | OpenTelemetry | Request count, duration, error rate per service |
| **Health Checks** | ASP.NET Core Health | DB + Redis availability per service |

Every request carries a `X-Correlation-Id` header, propagated through all services so a single trace can be reconstructed across logs from all services.

---

## 9. Security Architecture

```
Client → API Gateway → Validates JWT → Routes to service
                              │
                    Invalid token → 401 Unauthorized (never reaches service)

Services trust the gateway — they verify the JWT but don't re-authenticate.
Service-to-service calls use internal network only (not exposed publicly).
```

- **JWT** with RSA signing — short-lived access tokens (15 min)
- **Refresh Token Rotation** — single-use refresh tokens stored in DB
- **Role Claims** — `Employee`, `Manager`, `Admin` enforced at controller level
- **Secrets** — all via environment variables / GitHub Actions secrets (never in code)

---

## 10. Architecture Tests

Layer rules are **automatically enforced in CI** using ArchUnitNET:

```csharp
// These tests run on every PR and fail the build if violated
[Fact] void Domain_Has_No_EF_Core_Dependency()
[Fact] void Application_Has_No_Infrastructure_Dependency()  
[Fact] void Controllers_Only_Use_MediatR_Not_Repositories()
[Fact] void Domain_Entities_Have_No_Public_Setters()
```

This means the architecture cannot accidentally degrade over time — the tests enforce it permanently.

---

*See [ADRs](adr/) for the reasoning behind each key decision.*
