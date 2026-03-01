using System.Security.Cryptography;
using ExpenseFlow.Identity.Application.Interfaces;

namespace ExpenseFlow.Identity.Infrastructure.Services;

/// <summary>
/// PBKDF2-SHA256 password hasher with 100,000 iterations.
/// Format stored in DB:  {iterations}.{base64salt}.{base64hash}
/// This format lets us increase iterations in future without invalidating old passwords —
/// we re-hash on the next successful login if the iteration count is outdated.
/// </summary>
public sealed class PasswordHasherService : IPasswordHasher
{
    private const int    SaltSize       = 16;   // 128-bit salt
    private const int    HashSize       = 32;   // 256-bit hash
    private const int    Iterations     = 100_000;
    private const HashAlgorithmName Alg = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Alg, HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out var iterations))
            return false;

        var salt         = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Alg, HashSize);

        // Constant-time comparison — prevents timing attacks
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
