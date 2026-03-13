using ExpenseFlow.Identity.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Identity.IntegrationTests.Tests;

/// <summary>
/// xUnit collection fixture — ensures the Testcontainers SQL Server
/// and WebApplicationFactory are created ONCE and shared across all
/// tests in the [Collection("IdentityApi")] group.
///
/// Without this, each test class would spin up its own container,
/// making the suite ~10x slower.
/// </summary>
[CollectionDefinition("IdentityApi")]
public sealed class IdentityApiCollection
    : ICollectionFixture<IdentityApiFactory> { }
