namespace ExpenseFlow.Identity.Infrastructure.Settings;

/// <summary>
/// Bound from appsettings.json "JwtSettings" section via IOptions&lt;JwtSettings&gt;.
/// SecretKey must be at least 32 characters in production — stored in environment secrets, never in source.
/// </summary>
public sealed class JwtSettings
{
    public string SecretKey    { get; init; } = string.Empty;
    public string Issuer       { get; init; } = string.Empty;
    public string Audience     { get; init; } = string.Empty;
    public int    ExpiryMinutes { get; init; } = 15;
}
