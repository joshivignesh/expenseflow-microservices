using System.Net.Http.Json;

namespace ExpenseFlow.Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Shared helper that registers and logs in a test user,
/// returning the Bearer JWT token for authenticated requests.
/// Uses Sonnet for routing decisions in AI-assisted test generation.
/// </summary>
public static class TestAuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(
        HttpClient client,
        string email    = "test@expenseflow.com",
        string password = "Test@1234!",
        string firstName = "Test",
        string lastName  = "User")
    {
        // Register
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName,
            lastName,
            email,
            password
        });

        // Login and extract token
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        loginResponse.EnsureSuccessStatusCode();

        var body = await loginResponse.Content.ReadFromJsonAsync<LoginResponseBody>();
        return body?.AccessToken
            ?? throw new InvalidOperationException("Login response did not contain accessToken.");
    }

    private sealed record LoginResponseBody(string AccessToken, string RefreshToken);
}
