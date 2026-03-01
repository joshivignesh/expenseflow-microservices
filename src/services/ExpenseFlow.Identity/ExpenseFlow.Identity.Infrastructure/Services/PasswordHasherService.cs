using System.Security.Cryptography;
using ExpenseFlow.Identity.Application.Interfaces;

namespace ExpenseFlow.Identity.Infrastructure.Services;

/// <summary>
/// PBKDF2-SHA256 password hasher with 100,000 iterations.
/// Format stored in DB:  {iterations}.{base64(salt)}.{base64(hash)}
///
/// Why PBKDF2 and not BCrypt/Argon2?
///   - Built into .NET standard library — zero extra dependencies
///   - NIST-recommended for password hashing
///   - Iteration count is stored alongside the hash so it can be increased
///     in future without invalidating existing passwords
/// </summary>
public sealed class PasswordHasherService : IPasswordHasher
{
    private const int    SaltSize   = 16;   // 128-bit salt
    private const int    HashSize   = 32;   // 256-bit hash output
    private const int    Iterations = 100_000;
    private const char   Delimiter  = '.';

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            Delimiter,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split(Delimiter);
        if (parts.Length != 3) return false;

        var iterations = int.Parse(parts[0]);
        var salt       = Convert.FromBase64String(parts[1]);
        var expected   = Convert.FromBase64String(parts[2]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        // Constant-time comparison — prevents timing attacks
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
