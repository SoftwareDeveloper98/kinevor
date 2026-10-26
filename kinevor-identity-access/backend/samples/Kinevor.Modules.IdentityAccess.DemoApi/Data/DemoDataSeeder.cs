using Kinevor.Modules.IdentityAccess.Application;
using Kinevor.Modules.IdentityAccess.Domain;
using Kinevor.Modules.IdentityAccess.EntityFrameworkCore;

namespace Kinevor.Modules.IdentityAccess.DemoApi.Data;

/// <summary>Seeds deterministic demo accounts for every platform role.</summary>
public static class DemoDataSeeder
{
    /// <summary>Demo password shared by all seeded accounts.</summary>
    public const string DemoPassword = "Kinevor!2026";

    /// <summary>Adds the demo accounts when the database is empty.</summary>
    public static async Task SeedAsync(IdentityDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(hasher);

        if (db.UserAccounts.Any())
            return;

        var now = DateTime.UtcNow;
        var accounts = new[]
        {
            ("admin@kinevor.demo", "Nour Admin", "Admin"),
            ("staff@kinevor.demo", "Omar Staff", "Staff"),
            ("trainer@kinevor.demo", "Layla Trainer", "Trainer"),
            ("member@kinevor.demo", "Youssef Member", "Member"),
        };

        foreach (var (email, displayName, role) in accounts)
        {
            var account = UserAccount.Create(email, displayName, role, hasher.Hash(DemoPassword), now);
            if (account.IsSuccess)
                db.UserAccounts.Add(account.Value);
        }

        await db.SaveChangesAsync(ct);
    }
}
