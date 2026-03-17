# Expense Service

Manages the full lifecycle of expense claims — creation, submission, approval, and rejection.

## Responsibility
- Expense CRUD with a strict state machine (Draft → Submitted → Approved/Rejected)
- Ownership enforcement (only the creator can submit)
- Role-based approval (Manager/Admin only)
- Query-side projections via Dapper for the approval queue and user dashboards

## Layer Structure

```
ExpenseFlow.Expense/
├── ExpenseFlow.Expense.Domain/          # State machine, Money VO, domain events
├── ExpenseFlow.Expense.Application/     # CQRS — 4 commands, 3 queries
├── ExpenseFlow.Expense.Infrastructure/  # EF Core mapping, ExpenseRepository
└── ExpenseFlow.Expense.API/             # 7 REST endpoints, exception mapping
```

## Domain Layer

### `Expense` Aggregate Root — State Machine

The heart of the service. All state transitions go through aggregate methods — never direct property assignment.

```
Draft  ── Submit() ──►  Submitted  ── Approve(approverId) ─►  Approved
                                  └── Reject(id, reason) ─►  Rejected
```

Any invalid transition throws `InvalidExpenseStateException` → mapped to **422 Unprocessable Entity**.

**Why 422 and not 400?**
400 = malformed request. 422 = valid request, business rule violation. The semantics matter for frontend error handling.

### `Money` Value Object
Wraps `decimal Amount + string Currency`. Guards:
- Amount must be > 0
- Currency must be a 3-letter ISO 4217 code (INR, USD, GBP)
- `Add()`/`Subtract()` enforce same-currency — prevents accidental USD+INR arithmetic
- Rounds to 2 decimal places on creation
- `sealed record` gives structural equality for free

### `ExpenseDescription` Value Object
Max 500 characters, not empty. `MaxLength` constant shared between domain and EF configuration — single source of truth.

### Domain Events

| Event | Raised by | Downstream use |
|---|---|---|
| `ExpenseCreatedEvent` | `Expense.Create()` | Confirm receipt |
| `ExpenseSubmittedEvent` | `Submit()` | Notify approver |
| `ExpenseApprovedEvent` | `Approve()` | Trigger reimbursement |
| `ExpenseRejectedEvent` | `Reject()` | Notify submitter with reason |

## Application Layer

### Commands

| Command | Key behaviour |
|---|---|
| `CreateExpenseCommand` | `Money.Create()` + `ExpenseDescription.Create()` fires domain validation; `Expense.Create()` raises `ExpenseCreatedEvent` |
| `SubmitExpenseCommand` | Ownership check: `expense.SubmittedByUserId != requestingUserId` → 403 |
| `ApproveExpenseCommand` | Role check at API layer (`[Authorize(Roles="Manager,Admin")]`); domain enforces Submitted-only |
| `RejectExpenseCommand` | Mandatory reason validated by FluentValidation AND domain |

### Queries (all Dapper)

| Query | Returns | Used for |
|---|---|---|
| `GetExpenseByIdQuery` | `ExpenseDto?` | Expense detail page |
| `GetExpensesByUserQuery` | `IReadOnlyList<ExpenseDto>` | "My Expenses" dashboard |
| `GetExpensesByStatusQuery` | `IReadOnlyList<ExpenseDto>` | Manager approval queue |

SQL aliases: `Amount_Value AS Amount`, `Amount_Currency AS Currency` — maps EF owned-type column names to flat DTO properties.

## Infrastructure Layer

### `ExpenseConfiguration` — Two Owned Types

```csharp
builder.OwnsOne(e => e.Amount, money => {
    money.Property(m => m.Amount).HasColumnName("Amount_Value").HasColumnType("decimal(18,2)");
    money.Property(m => m.Currency).HasColumnName("Amount_Currency").HasMaxLength(3);
});
builder.OwnsOne(e => e.Description, desc => {
    desc.Property(d => d.Value).HasColumnName("Description").HasMaxLength(500);
});
```

Both value objects inline into the `Expenses` table — no join tables, no extra round trips.

### Indexes
- `IX_Expenses_SubmittedByUserId` — "My Expenses" query
- `IX_Expenses_Status` — approval queue query

## API Layer

### Endpoints

| Method | Route | Auth | Role |
|---|---|---|---|
| POST | `/api/expenses` | JWT | Any |
| POST | `/api/expenses/{id}/submit` | JWT | Owner only |
| POST | `/api/expenses/{id}/approve` | JWT | Manager/Admin |
| POST | `/api/expenses/{id}/reject` | JWT | Manager/Admin |
| GET | `/api/expenses/{id}` | JWT | Any |
| GET | `/api/expenses/my` | JWT | Any |
| GET | `/api/expenses/pending` | JWT | Manager/Admin |

`CurrentUserId` is read from `ClaimTypes.NameIdentifier` in the JWT — tamper-proof, never from request body.

### Exception → HTTP Mapping

| Exception | HTTP | Reason |
|---|---|---|
| `ValidationException` | 400 | Malformed input |
| `ExpenseNotFoundException` | 404 | Not found |
| `InvalidExpenseStateException` | 422 | Business rule violation |
| `UnauthorizedAccessException` | 403 | Ownership or role failure |
