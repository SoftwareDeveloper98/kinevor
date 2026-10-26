using System.Security.Cryptography;
using System.Text;

namespace Kinevor.Modules.IdentityAccess.Application;

/// <summary>PBKDF2-HMAC-SHA256 password hasher with 120,000 iterations.</summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 600_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    /// <summary>Encodes a password as <c>iterations.salt.hash</c> in Base64.</summary>
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashBytes);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>Verifies a password against an encoded hash using a fixed-time comparison.</summary>
    public bool Verify(string password, string encodedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(encodedHash))
            return false;

        var parts = encodedHash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations) || iterations < 10_000)
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>Whether the stored hash was produced with fewer than the current iteration count.</summary>
    public bool NeedsRehash(string encodedHash)
    {
        if (string.IsNullOrWhiteSpace(encodedHash))
            return true;

        var parts = encodedHash.Split('.');
        return parts.Length != 3
            || !int.TryParse(parts[0], out var iterations)
            || iterations < Iterations;
    }
}
