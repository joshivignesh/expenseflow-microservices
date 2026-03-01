namespace ExpenseFlow.Identity.Infrastructure.Settings;

/// <summary>
/// Bound from appsettings.json → "JwtSettings" section.
/// Never commit real values — use secrets or environment variables in production.
/// </summary>
public sealed class JwtSettings
{
    public string SecretKey               { get; init; } = string.Empty;
    public string Issuer                  { get; init; } = string.Empty;
    public string Audience                { get; init; } = string.Empty;
    public int    AccessTokenExpiryMinutes { get; init; } = 15;
    public int    RefreshTokenExpiryDays  { get; init; } = 7;
}
