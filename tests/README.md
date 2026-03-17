# Integration Tests

Full-stack integration tests using **WebApplicationFactory** + **Testcontainers**. No mocks — every test hits a real SQL Server database in a Docker container.

## Architectural Decision: Why Integration Tests over Unit Tests?

Unit tests mock the database, mock the repository, mock MediatR. They test logic in isolation but can miss:
- EF Core configuration bugs (wrong column type, missing index)
- Middleware pipeline ordering errors
- FluentValidation not being registered correctly
- DI registration mistakes

Integration tests boot the **entire application** in-process and exercise it through HTTP. If `Program.cs` has a wiring bug, the test fails. If the EF mapping is wrong, the test fails. They give much higher confidence that the system actually works end-to-end.

## How It Works

### `WebApplicationFactory<Program>`
Boots the real `Program.cs` in-process. Same DI container, same middleware pipeline, same EF Core config. `ConfigureWebHost` surgically replaces one thing: the `DbContextOptions` registration is swapped for a Testcontainers connection string.

### Testcontainers (`MsSqlContainer`)
Spins up a real `mcr.microsoft.com/mssql/server:2022-latest` Docker container on test startup. EF Core migrations run against it. On teardown the container is destroyed. Each test class shares one container (via `IClassFixture`) — fast without data collision because each test uses unique emails and user IDs.

### Test Isolation Strategy
- Identity tests: `UniqueEmail()` generates a UUID-based email per test — no conflicts
- Expense tests: `NewUserId()` generates a fresh `Guid` per test — expenses are owned independently
- No database cleanup between tests — insert-only pattern keeps tests fast and deterministic

## JWT Generation in Expense Tests

The Expense service needs authenticated requests but the Identity Service isn't running. `IntegrationTestBase.GenerateJwt()` builds a real HS256-signed JWT using the same `TestJwtSecret` injected into the factory. `AuthenticateAs(userId, role)` sets `Client.DefaultRequestHeaders.Authorization` for all subsequent requests in that test.

Switching roles mid-test (employee creates, manager approves) is just calling `AuthenticateAs` again.

## Key Test Scenarios

### Identity
- Register → 201 + tokens + Location header
- Duplicate email → 409 Conflict
- Weak password → 400 + validation error array
- Login with correct credentials → 200 + tokens
- Login with wrong password → 401 (same as unknown email — never reveal existence)
- Get profile with valid JWT → 200
- Get profile without JWT → 401
- Get profile with valid JWT but non-existent ID → 404

### Expense Lifecycle
- Create → Submit → Approve: full happy path across two users and two roles
- Create → Submit → Reject with reason
- Approve a Draft (skipping Submit) → **422** — proves the state machine holds through the full HTTP stack
- Submit another user's expense → **403** — proves ownership enforcement
- Reject without reason → **400** — proves FluentValidation fires

## Running Locally

```bash
# Docker must be running

# Identity tests
dotnet test tests/ExpenseFlow.Identity.IntegrationTests/

# Expense tests
dotnet test tests/ExpenseFlow.Expense.IntegrationTests/
```

## CI Pipeline
Two parallel jobs on `ubuntu-latest` (Docker pre-installed). Results published as `.trx` files rendered in the PR checks panel via `dorny/test-reporter`.
