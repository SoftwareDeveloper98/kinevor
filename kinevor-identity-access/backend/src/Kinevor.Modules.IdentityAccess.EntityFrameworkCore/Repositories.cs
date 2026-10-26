using Kinevor.Modules.IdentityAccess.Application;
using Kinevor.Modules.IdentityAccess.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.IdentityAccess.EntityFrameworkCore;

/// <summary>EF Core account repository.</summary>
public sealed class UserAccountRepository(IdentityDbContext db) : IUserAccountRepository
{
    /// <summary>Finds an account by normalised e-mail.</summary>
    public async Task<UserAccount?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalised = (email ?? string.Empty).Trim().ToLowerInvariant();
        var accounts = await db.UserAccounts.ToListAsync(ct);
        return accounts.FirstOrDefault(account =>
            string.Equals(account.Email.Value, normalised, StringComparison.Ordinal));
    }

    /// <summary>Finds an account by id.</summary>
    public Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.UserAccounts.FirstOrDefaultAsync(account => account.Id == id, ct);

    /// <summary>Lists accounts ordered by display name.</summary>
    public async Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken ct = default) =>
        await db.UserAccounts.AsNoTracking().OrderBy(account => account.DisplayName).ToListAsync(ct);

    /// <summary>Tracks a new account.</summary>
    public void Add(UserAccount account) => db.UserAccounts.Add(account);

    /// <summary>Marks an account as modified.</summary>
    public void Update(UserAccount account) => db.UserAccounts.Update(account);
}

/// <summary>EF Core refresh-token repository.</summary>
public sealed class RefreshTokenRepository(IdentityDbContext db) : IRefreshTokenRepository
{
    /// <summary>Finds a refresh token by hash.</summary>
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(token => token.TokenHash == tokenHash, ct);

    /// <summary>Tracks a new refresh token.</summary>
    public void Add(RefreshToken token) => db.RefreshTokens.Add(token);
}
