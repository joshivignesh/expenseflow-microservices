# ADR-001: Use Dapper for CQRS Read Side (Not EF Core)

**Date:** 2026-02-22  
**Status:** Accepted  
**Author:** Vignesh Joshi

---

## Context

In a CQRS architecture, the Read side (Queries) and Write side (Commands) have fundamentally different needs:
- **Write side** benefits from EF Core's change tracking, domain model enforcement, and migrations
- **Read side** just needs to return flat data as fast as possible — no domain rules apply

EF Core's change tracking and object materialisation add overhead that is unnecessary for pure reads.

## Decision

Use **Dapper** for all Query handlers. Use **EF Core** only for Command handlers.

```csharp
// Query handler — Dapper, raw SQL, returns flat ViewModel
public async Task<PagedResult<ExpenseSummaryViewModel>> Handle(GetExpensesByUserQuery query, CancellationToken ct)
{
    const string sql = @"
        SELECT e.Id, e.Amount_Value AS Amount, e.Status, e.SubmittedAt
        FROM Expenses e
        WHERE e.SubmittedById = @UserId
        ORDER BY e.SubmittedAt DESC
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

    var results = await _db.QueryAsync<ExpenseSummaryViewModel>(sql, query);
    return results.ToPagedResult(query.Page, query.PageSize);
}
```

## Consequences

**Positive:**
- Significantly faster reads — no change tracking, no EF materialisation overhead
- SQL is explicit and reviewable — no surprises from generated queries
- ViewModels can be shaped exactly to what the UI needs (no over/under-fetching)
- Follows Microsoft's eShopOnContainers reference implementation exactly

**Negative:**
- Two data access technologies to maintain
- SQL strings are not type-safe (mitigated by integration tests)

## References
- Microsoft Microservices Guide — Chapter 7: Implementing reads/queries in a CQRS microservice
- eShopOnContainers source: `Ordering.API` query handlers
