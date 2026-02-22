# ADR-004: Use TestContainers for Integration Tests (Not In-Memory DB)

**Date:** 2026-02-22  
**Status:** Accepted  
**Author:** Vignesh Joshi

---

## Context

Integration tests for the Infrastructure layer (EF Core repositories, Dapper queries) need a database. Two common approaches:
1. **In-memory database** (e.g., EF Core InMemory provider or SQLite)
2. **Real SQL Server** spun up via Docker (TestContainers)

## Decision

Use **TestContainers** to spin up a real SQL Server instance for each integration test run.

```csharp
public class ExpenseRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        // Run migrations against real SQL Server
    }

    [Fact]
    public async Task Should_Persist_And_Retrieve_Expense()
    {
        // Tests run against REAL SQL Server — no surprises in production
    }
}
```

## Consequences

**Positive:**
- Tests run against the **exact same database engine** as production
- Catches SQL Server-specific issues (data types, collation, constraints) that in-memory DBs hide
- Migrations are tested as part of the test run
- Full confidence that what works in tests works in production

**Negative:**
- Slower than in-memory tests (~5-10s container startup)
- Requires Docker to be running during test execution
- CI must have Docker available (standard on GitHub Actions)

## Why Not In-Memory?

EF Core InMemory and SQLite don't support many SQL Server features:
- No raw SQL execution (Dapper queries fail)
- No stored procedures
- Different NULL handling
- No transaction isolation levels

These silent differences mean tests pass locally but fail in production.

## References
- TestContainers for .NET: https://dotnet.testcontainers.org
- Microsoft Testing Guide: Use real dependencies in integration tests
