namespace ExpenseFlow.Shared.Application.Interfaces;

/// <summary>
/// Unit of Work pattern — groups multiple repository operations into a single transaction.
/// Ensures that either ALL changes are saved, or NONE are (atomicity).
///
/// SaveEntitiesAsync also dispatches domain events after the transaction commits.
/// This is the correct ordering: persist state FIRST, then publish events.
///
/// Source: Microsoft Microservices Guide, Chapter 7 — Implementing the infrastructure
/// persistence layer with Entity Framework Core.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Persists all pending changes and dispatches accumulated domain events.
    /// Returns true if any rows were affected.
    /// </summary>
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}
