using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExpenseFlow.Identity.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Identity.IntegrationTests.Tests.AuthController;

/// <summary>
/// Integration tests for POST /api/auth/register.
/// Verifies the full stack: HTTP → Controller → MediatR → Handler → EF Core → SQL Server (container).
/// </summary>
public sealed class RegisterTests : IntegrationTestBase
{
    public RegisterTests(IdentityWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ValidPayload_Returns201WithTokens()
    {
        // Arrange
        var request = new
        {
            firstName = "Vignesh",
            lastName  = "Joshi",
            email     = UniqueEmail(),
            password  = "Test@1234!"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
        body.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409Conflict()
    {
        // Arrange — register once
        var email = UniqueEmail();
        var request = new { firstName = "A", lastName = "B", email, password = "Test@1234!" };
        await Client.PostAsJsonAsync("/api/auth/register", request);

        // Act — try to register again with same email
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400WithValidationErrors()
    {
        // Arrange
        var request = new
        {
            firstName = "Test",
            lastName  = "User",
            email     = UniqueEmail(),
            password  = "weak"   // fails FluentValidation rules
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        body!.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_MissingEmail_Returns400()
    {
        var request = new { firstName = "Test", lastName = "User", password = "Test@1234!" };
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Response / problem detail models ───────────────────────────────────
    private sealed record AuthResponse(
        Guid   UserId,
        string AccessToken,
        string RefreshToken,
        DateTime ExpiresAt);

    private sealed record ProblemDetails(
        string   Title,
        int      Status,
        string[] Errors);
}
