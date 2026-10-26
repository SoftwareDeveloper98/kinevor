using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.IdentityAccess.Domain;

/// <summary>Single-use refresh token stored as a SHA-256 hash.</summary>
public sealed class RefreshToken : AggregateRoot
{
    /// <summary>Account the token was issued to.</summary>
    public Guid UserId { get; private set; }

    /// <summary>SHA-256 hash of the opaque token.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>UTC expiration timestamp.</summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC revocation timestamp, when revoked.</summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>Whether the token can still be exchanged.</summary>
    public bool IsActive(DateTime now) => RevokedAt is null && ExpiresAt > now;

    private RefreshToken() { }

    /// <summary>Creates a refresh token record.</summary>
    public static RefreshToken Create(Guid userId, string tokenHash, DateTime now, int lifetimeDays = 30) =>
        new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(lifetimeDays),
        };

    /// <summary>Revokes the token if it is still active.</summary>
    public void Revoke(DateTime now) => RevokedAt ??= now;
}
