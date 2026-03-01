using Microsoft.EntityFrameworkCore;
using ExpenseFlow.Identity.Domain.Entities;
using ExpenseFlow.Shared.Domain;

namespace ExpenseFlow.Identity.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the Identity bounded context.
/// Owns only the tables that belong to this service — no cross-service references.
/// Domain events are dispatched after SaveChanges via the IDomainEventDispatcher.
/// </summary>
public sealed class IdentityDbContext : DbContext
{
    private readonly IDomainEventDispatcher _dispatcher;

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IDomainEventDispatcher dispatcher)
        : base(options)
    {
        _dispatcher = dispatcher;
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration<T> classes in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }

    /// <summary>
    /// Saves changes and dispatches any domain events raised during the operation.
    /// Call this instead of SaveChangesAsync when domain events should be published.
    /// </summary>
    public async Task<bool> SaveEntitiesAsync(CancellationToken ct = default)
    {
        await base.SaveChangesAsync(ct);

        // Collect and clear domain events from all tracked aggregates
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            entry.Entity.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await _dispatcher.DispatchAsync(domainEvent, ct);

        return true;
    }
}
