using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ExpenseFlow.Identity.Application.Interfaces;
using ExpenseFlow.Identity.Domain.Entities;
using ExpenseFlow.Identity.Infrastructure.Settings;

namespace ExpenseFlow.Identity.Infrastructure.Services;

/// <summary>
/// Generates a short-lived JWT access token and a long-lived opaque refresh token.
/// Access token: 15 minutes, signed with HMAC-SHA256.
/// Refresh token: 7 days, cryptographically random (not a JWT).
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
        => _settings = settings.Value;

    public (string AccessToken, string RefreshToken, DateTime ExpiresAt) GenerateTokens(User user)
    {
        var accessToken  = BuildAccessToken(user);
        var refreshToken = BuildRefreshToken();
        var expiresAt    = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays);

        return (accessToken, refreshToken, expiresAt);
    }

    // ── private helpers ────────────────────────────────────────────────────────

    private string BuildAccessToken(User user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name,               user.FullName),
            new Claim(ClaimTypes.Role,               user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             _settings.Issuer,
            audience:           _settings.Audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Refresh token is a plain random byte string — NOT a JWT.
    /// Storing it hashed in the DB is a future hardening step.
    /// </summary>
    private static string BuildRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
