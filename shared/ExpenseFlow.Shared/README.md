# ExpenseFlow.Shared

Shared building blocks used across **all** bounded contexts. Contains no business logic — only contracts and base infrastructure that every service depends on.

## Architectural Decision

**Why a shared library instead of NuGet packages?**

For a portfolio microservices project, a shared `ProjectReference` is faster to iterate on — no publish/version cycle. In a real multi-team setup, these would be published as versioned NuGet packages to a private feed so each service can pin to its own version.

## Contents

### `AggregateRoot`
Base class for all aggregate roots. Holds the `DomainEvents` collection.
- `AddDomainEvent(IDomainEvent)` — registers a domain event to be dispatched after persistence
- `ClearDomainEvents()` — called by the DbContext after dispatch
- `DomainEvents` is `IReadOnlyList` — immutable from outside the aggregate

**Why post-persistence dispatch?**
Events are dispatched *after* `SaveChangesAsync` succeeds. If the DB write fails, no events fire — preventing ghost events with no corresponding state change. This is the "outbox-lite" pattern without the full transactional outbox infrastructure.

### `IDomainEvent`
Marker interface. All domain events implement this. Used by `IDomainEventDispatcher` for type-safe dispatch via MediatR's `INotification`.

### `IUnitOfWork`
Single method: `SaveEntitiesAsync(CancellationToken)`. Implemented by each service's DbContext. Command handlers call this instead of `SaveChangesAsync` so domain events are always dispatched after persistence.

### `IDomainEventDispatcher`
Abstraction over MediatR's `IPublisher`. Decouples the domain from MediatR — domain layer never references MediatR directly.

## Dependency Rule

```
Shared  ←  Domain  ←  Application  ←  Infrastructure  ←  API
```

Shared has **zero** external NuGet dependencies. All other layers depend on Shared transitively.
