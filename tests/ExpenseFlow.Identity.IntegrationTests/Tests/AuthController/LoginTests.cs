using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExpenseFlow.Identity.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Identity.IntegrationTests.Tests.AuthController;

/// <summary>
/// Integration tests for POST /api/auth/login.
/// </summary>
public sealed class LoginTests : IntegrationTestBase
{
    public LoginTests(IdentityWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        // Arrange — register first so the user exists
        var email    = UniqueEmail();
        var password = "Test@1234!";
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Login", lastName = "Test",
            email, password
        });

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, password });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        // Arrange
        var email = UniqueEmail();
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Login", lastName = "Fail",
            email, password = "Test@1234!"
        });

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPassword!" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonExistentEmail_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new { email = UniqueEmail(), password = "Test@1234!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record AuthResponse(
        Guid UserId, string AccessToken, string RefreshToken, DateTime ExpiresAt);
}
