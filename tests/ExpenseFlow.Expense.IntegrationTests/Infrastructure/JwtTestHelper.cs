using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseFlow.Expense.IntegrationTests.Infrastructure;

/// <summary>
/// Generates real signed JWTs for test authentication.
/// Uses the same secret key configured in ExpenseApiFactory
/// so the API's JWT middleware accepts them without any mocking.
///
/// Model role: Haiku — simple, fast token generation. U0001f493
/// </summary>
public static class JwtTestHelper
{
    private const string SecretKey = "test-secret-key-min-32-chars-long!!";
    private const string Issuer    = "ExpenseFlow.Identity";
    private const string Audience  = "ExpenseFlow.Client";

    public static string GenerateToken(
        Guid   userId,
        string role = "Employee")
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email,          $"testuser_{userId}@test.com"),
            new Claim(ClaimTypes.Role,           role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             Issuer,
            audience:           Audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
