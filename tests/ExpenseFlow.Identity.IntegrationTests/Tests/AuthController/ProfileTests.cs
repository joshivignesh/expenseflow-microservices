using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ExpenseFlow.Identity.IntegrationTests.Infrastructure;

namespace ExpenseFlow.Identity.IntegrationTests.Tests.AuthController;

/// <summary>
/// Integration tests for GET /api/auth/profile/{userId}.
/// </summary>
public sealed class ProfileTests : IntegrationTestBase
{
    public ProfileTests(IdentityWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task GetProfile_WithValidToken_Returns200()
    {
        // Arrange — register and login to get a real token
        var email    = UniqueEmail();
        var password = "Test@1234!";

        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Profile", lastName = "Test", email, password
        });

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login",
            new { email, password });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Act — call profile with the real JWT
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        var response = await Client.GetAsync(
            $"/api/auth/profile/{loginBody.UserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        profile!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetProfile_WithNoToken_Returns401()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/auth/profile/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_NonExistentUser_Returns404()
    {
        // Register to get a valid token, then query a random non-existent ID
        var email = UniqueEmail();
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Ghost", lastName = "User", email, password = "Test@1234!"
        });
        var login = await Client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "Test@1234!" });
        var loginBody = await login.Content.ReadFromJsonAsync<AuthResponse>();

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody!.AccessToken);

        var response = await Client.GetAsync(
            $"/api/auth/profile/{Guid.NewGuid()}");  // random unknown ID

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record AuthResponse(
        Guid UserId, string AccessToken, string RefreshToken, DateTime ExpiresAt);

    private sealed record UserProfileDto(
        Guid UserId, string FirstName, string LastName, string Email, string Role);
}
