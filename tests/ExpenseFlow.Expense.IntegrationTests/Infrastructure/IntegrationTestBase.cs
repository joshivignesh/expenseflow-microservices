using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseFlow.Expense.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for all Expense integration tests.
/// Provides JWT generation helpers so tests can authenticate as
/// different user roles without needing the Identity Service running.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<ExpenseWebAppFactory>
{
    protected readonly HttpClient Client;

    protected IntegrationTestBase(ExpenseWebAppFactory factory)
    {
        Client = factory.CreateClient();
    }

    /// <summary>
    /// Generates a signed JWT for the given user and sets it on the HttpClient.
    /// Uses the same secret as the test factory so the Expense API accepts it.
    /// </summary>
    protected void AuthenticateAs(Guid userId, string role = "Employee")
    {
        var token = GenerateJwt(userId, role);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected static string GenerateJwt(Guid userId, string role)
    {
        var key   = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(ExpenseWebAppFactory.TestJwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role,           role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             "ExpenseFlow.Identity",
            audience:           "ExpenseFlow.Client",
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected static Guid NewUserId() => Guid.NewGuid();
}
