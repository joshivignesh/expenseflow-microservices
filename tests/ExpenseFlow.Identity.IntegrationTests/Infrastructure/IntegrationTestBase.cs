using System.Net.Http.Json;

namespace ExpenseFlow.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for all Identity integration tests.
/// Provides a pre-configured HttpClient and helper methods.
/// Uses IClassFixture so the factory (and SQL container) is shared
/// across all tests in the same class — one container per class, not per test.
/// </summary>
public abstract class IntegrationTestBase
    : IClassFixture<IdentityWebAppFactory>
{
    protected readonly HttpClient Client;

    protected IntegrationTestBase(IdentityWebAppFactory factory)
    {
        Client = factory.CreateClient();
    }

    /// <summary>Registers a user and returns the parsed response body.</summary>
    protected async Task<T?> PostAsync<T>(string url, object body)
    {
        var response = await Client.PostAsJsonAsync(url, body);
        return await response.Content.ReadFromJsonAsync<T>();
    }

    /// <summary>Returns a unique email for each test to avoid duplicate conflicts.</summary>
    protected static string UniqueEmail()
        => $"test_{Guid.NewGuid():N}@expenseflow.test";
}
