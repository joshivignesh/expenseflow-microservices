# Architecture Docs

Architectural Decision Records (ADRs) and design documentation for ExpenseFlow.

## ADR Index

| ADR | Decision | Status |
|---|---|---|
| ADR-001 | Clean Architecture as the primary structural pattern | Accepted |
| ADR-002 | MediatR for CQRS command/query separation | Accepted |
| ADR-003 | EF Core for writes, Dapper for reads | Accepted |
| ADR-004 | YARP as API Gateway | Accepted |
| ADR-005 | JWT HS256 with shared secret across services | Accepted |
| ADR-006 | Testcontainers for integration testing | Accepted |
| ADR-007 | OpenTelemetry for vendor-neutral observability | Accepted |

## Core Principles

### 1. Dependency Rule (Clean Architecture)

```
Domain ← Application ← Infrastructure ← API
  ↑
 Shared
```

Arrows point inward. Inner layers have zero knowledge of outer layers. Domain has no NuGet dependencies. This is enforced structurally — `Domain.csproj` references only `Shared.csproj`.

**Practical benefit:** The domain and application layers are fully testable without spinning up a database or web server. Infrastructure can be swapped (SQL Server → PostgreSQL) without touching business logic.

### 2. Aggregate Roots Protect Invariants

No public setters on aggregate properties. All state changes go through methods (`Submit()`, `Approve()`, `Reject()`). The aggregate is the consistency boundary — it is impossible to persist an `Expense` in an invalid state if you only use the aggregate's methods.

### 3. Domain Events Over Direct Coupling

Services communicate intent through domain events raised by aggregates, dispatched after persistence. `ExpenseApprovedEvent` could trigger a reimbursement service without `ExpenseService` knowing that service exists. This is the basis for eventual consistency and event-driven extension.

### 4. CQRS Split (EF Core + Dapper)

Command side: EF Core with full change tracking. Needed for domain object loading, optimistic concurrency, and migration management.

Query side: Raw Dapper SQL. Reads are the majority of traffic. EF Core's change tracking adds overhead with no benefit for read-only projections. Dapper maps directly to flat DTOs — faster, simpler SQL.

### 5. Validate at the Boundary, Enforce in the Domain

FluentValidation catches malformed HTTP requests at the API boundary (missing fields, wrong format) — returns 400. Domain methods enforce business rules (wrong state transition, negative amount) — throws domain exceptions mapped to 422. Two different failure modes, two different status codes, two different layers of responsibility.

### 6. Observability is Not Optional

Structured JSON logs (Serilog) → stdout → aggregated by Docker/cloud platform.
Distributed traces (OpenTelemetry OTLP) → Jaeger — follow a request across gateway → identity → database.
Metrics (Prometheus) → Grafana — request rate, error rate, DB query latency per service.
`X-Correlation-ID` threads through every log line so a single browser request can be traced end-to-end.

## Service Boundaries

The two services share a SQL Server host in development but use **separate databases**:
- `ExpenseFlowIdentityDb` — owned by Identity Service
- `ExpenseFlowExpenseDb` — owned by Expense Service

No cross-database queries. The Expense Service stores `SubmittedByUserId` as a plain `Guid` — it does not join to the Identity database to resolve user details. This preserves the microservice boundary. In a production multi-team setup, these would be separate Azure SQL instances.
