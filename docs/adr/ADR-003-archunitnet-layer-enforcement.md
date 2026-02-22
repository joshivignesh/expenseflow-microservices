# ADR-003: Enforce Clean Architecture Layers with ArchUnitNET in CI

**Date:** 2026-02-22  
**Status:** Accepted  
**Author:** Vignesh Joshi

---

## Context

Clean Architecture requires strict layer dependency rules:
- Domain must not depend on Infrastructure or Application
- Application must not depend on Infrastructure
- Controllers must not bypass Application and call Repositories directly

Without enforcement, these rules get broken accidentally over time — especially in a team setting. A developer under deadline pressure might add a quick EF Core call directly in the Domain layer, violating the entire architecture.

## Decision

Use **ArchUnitNET** to write automated tests that enforce layer rules, and run them in **every CI build**.

```csharp
[Fact]
public void Domain_Should_Not_Depend_On_Infrastructure()
{
    Types.InAssembly(DomainAssembly)
        .ShouldNot()
        .HaveDependencyOn("Microsoft.EntityFrameworkCore")
        .GetResult()
        .IsSuccessful.Should().BeTrue();
}

[Fact]
public void Application_Should_Not_Depend_On_Infrastructure()
{
    Types.InAssembly(ApplicationAssembly)
        .ShouldNot()
        .HaveDependencyOn("ExpenseFlow.Expense.Infrastructure")
        .GetResult()
        .IsSuccessful.Should().BeTrue();
}

[Fact]
public void Controllers_Should_Not_Use_Repositories_Directly()
{
    Types.InAssembly(ApiAssembly)
        .That().AreAssignableTo(typeof(ControllerBase))
        .ShouldNot()
        .HaveDependencyOn("IExpenseRepository")
        .GetResult()
        .IsSuccessful.Should().BeTrue();
}
```

## Consequences

**Positive:**
- Architecture is **permanently protected** — violations break the build immediately
- New developers can't accidentally break the design
- Architecture docs and reality stay in sync
- Tests act as **living documentation** of the intended structure

**Negative:**
- Small learning curve for ArchUnitNET API
- Tests need updating if intentional architectural changes are made

## References
- Microsoft ASP.NET Core Architecture Guide — Chapter 5: Clean Architecture
- ArchUnitNET documentation: https://archunitnet.readthedocs.io
