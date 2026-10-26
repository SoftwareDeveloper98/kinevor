using Kinevor.Modules.IdentityAccess.Application;
using Kinevor.Modules.IdentityAccess.Domain;

namespace Kinevor.Modules.IdentityAccess.Tests;

/// <summary>Domain and security primitives.</summary>
public class IdentityDomainTests
{
    /// <summary>Accounts reject unsupported roles.</summary>
    [Fact]
    public void Create_RejectsUnknownRole()
    {
        var result = UserAccount.Create(
            "owner@kinevor.demo", "Owner", "SuperUser", "encoded-hash", DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Role", result.Error.Code);
    }

    /// <summary>Accounts normalise e-mail addresses.</summary>
    [Fact]
    public void Create_NormalisesEmail()
    {
        var result = UserAccount.Create(
            "  OWNER@Kinevor.Demo ", "Owner", "Admin", "encoded-hash", DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal("owner@kinevor.demo", result.Value.Email.Value);
    }

    /// <summary>PBKDF2 hashes verify only the correct password.</summary>
    [Fact]
    public void PasswordHasher_RoundTrips()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("Kinevor!2026");

        Assert.True(hasher.Verify("Kinevor!2026", hash));
        Assert.False(hasher.Verify("wrong", hash));
        Assert.False(hasher.Verify("Kinevor!2026", "malformed"));
    }

    /// <summary>Refresh tokens stop being active once revoked.</summary>
    [Fact]
    public void RefreshToken_RevokeStopsExchange()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", now);

        Assert.True(token.IsActive(now));
        token.Revoke(now.AddMinutes(1));
        Assert.False(token.IsActive(now.AddMinutes(2)));
    }
}
