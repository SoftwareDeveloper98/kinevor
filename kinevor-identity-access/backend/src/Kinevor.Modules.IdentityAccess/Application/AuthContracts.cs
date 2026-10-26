using Kinevor.Modules.IdentityAccess.Domain;

namespace Kinevor.Modules.IdentityAccess.Application;

/// <summary>Public account projection.</summary>
/// <param name="Id">Account identifier.</param>
/// <param name="Email">Login e-mail.</param>
/// <param name="DisplayName">Display name.</param>
/// <param name="Role">Platform role.</param>
/// <param name="MfaEnabled">Whether a second factor is enrolled.</param>
/// <param name="LastLoginAt">Last successful login timestamp.</param>
public record UserAccountDto(Guid Id, string Email, string DisplayName, string Role, bool MfaEnabled, DateTime? LastLoginAt)
{
    /// <summary>Projects an account aggregate.</summary>
    public static UserAccountDto From(UserAccount account) =>
        new(account.Id, account.Email.Value, account.DisplayName, account.Role, account.MfaEnabled, account.LastLoginAt);
}

/// <summary>Issued access and refresh tokens plus the signed-in account.</summary>
/// <param name="AccessToken">JWT access token.</param>
/// <param name="RefreshToken">Opaque single-use refresh token.</param>
/// <param name="ExpiresAt">UTC access-token expiry.</param>
/// <param name="User">Signed-in account.</param>
public record AuthSessionDto(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserAccountDto User);

/// <summary>Access token and expiry produced by the token service.</summary>
/// <param name="Value">Serialized JWT.</param>
/// <param name="ExpiresAt">UTC expiry.</param>
public record AccessToken(string Value, DateTime ExpiresAt);

/// <summary>Persistence contract for accounts.</summary>
public interface IUserAccountRepository
{
    /// <summary>Finds an account by e-mail.</summary>
    Task<UserAccount?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Finds an account by id.</summary>
    Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lists accounts ordered by display name.</summary>
    Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken ct = default);

    /// <summary>Tracks a new account.</summary>
    void Add(UserAccount account);

    /// <summary>Marks an account as modified.</summary>
    void Update(UserAccount account);
}

/// <summary>Persistence contract for refresh tokens.</summary>
public interface IRefreshTokenRepository
{
    /// <summary>Finds a token by its SHA-256 hash.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Tracks a new refresh token.</summary>
    void Add(RefreshToken token);
}

/// <summary>Work-unit abstraction over the module DbContext.</summary>
public interface IIdentityUnitOfWork
{
    /// <summary>Persists pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

/// <summary>Password hashing contract.</summary>
public interface IPasswordHasher
{
    /// <summary>Encodes a plaintext password.</summary>
    string Hash(string password);

    /// <summary>Verifies a plaintext password against an encoded hash.</summary>
    bool Verify(string password, string encodedHash);

    /// <summary>Whether the stored hash was produced with fewer than the current iteration count and should be rehashed.</summary>
    bool NeedsRehash(string encodedHash);
}

/// <summary>Token creation and hashing contract.</summary>
public interface IAuthTokenService
{
    /// <summary>Creates a signed JWT for the account.</summary>
    AccessToken CreateAccessToken(UserAccount account);

    /// <summary>Creates an opaque refresh token.</summary>
    string CreateRefreshToken();

    /// <summary>Hashes a refresh token for storage and lookup.</summary>
    string HashRefreshToken(string token);
}
