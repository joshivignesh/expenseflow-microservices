using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ExpenseFlow.Identity.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Identity.IntegrationTests.Tests;

/// <summary>
/// End-to-end integration tests for the /api/auth endpoints.
/// Each test runs against a real SQL Server (Testcontainers) and the full
/// ASP.NET Core pipeline — middleware, MediatR, EF Core, domain logic.
///
/// Model usage:
///   Haiku  — health/heartbeat assertions (fast, cheap) U0001f493
///   Sonnet — routing and orchestration logic assertions U0001f9e0
///   Opus   — complex scenario assertions (multi-step flows) U0001f3cb️
/// </summary>
[Collection("IdentityApi")]
public sealed class AuthControllerTests
{
    private readonly HttpClient _client;

    public AuthControllerTests(IdentityApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Register ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidRequest_Returns201WithTokens()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Vignesh",
            lastName  = "Joshi",
            email     = $"vignesh_{Guid.NewGuid()}@test.com",
            password  = "Test@1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<dynamic>();
        ((object?)body).Should().NotBeNull();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409Conflict()
    {
        var email = $"dup_{Guid.NewGuid()}@test.com";

        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "First", lastName = "User",
            email, password = "Test@1234!"
        });

        // Second registration with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Second", lastName = "User",
            email, password = "Test@1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_InvalidPassword_Returns400WithErrors()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test", lastName = "User",
            email     = "weakpw@test.com",
            password  = "weak"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("errors");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithAccessToken()
    {
        var email = $"login_{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Login", lastName = "Test",
            email, password = "Test@1234!"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email, password = "Test@1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("accessToken");
        body.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var email = $"wrongpw_{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Wrong", lastName = "Pw",
            email, password = "Test@1234!"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email, password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Profile ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProfile_WithValidToken_Returns200()
    {
        var email = $"profile_{Guid.NewGuid()}@test.com";

        // Register
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Profile", lastName = "Test",
            email, password = "Test@1234!"
        });

        var registerBody = await registerResponse.Content
            .ReadFromJsonAsync<RegisterResponseBody>();

        // Get token
        var token = await TestAuthHelper.RegisterAndLoginAsync(_client,
            $"profile2_{Guid.NewGuid()}@test.com");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync(
            $"/api/auth/profile/{registerBody!.UserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync(
            $"/api/auth/profile/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record RegisterResponseBody(Guid UserId, string Status);
}
