using Kinevor.Modules.CheckInsAccess.Application;
using Kinevor.Modules.CheckInsAccess.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.CheckInsAccess.EntityFrameworkCore;

/// <summary>EF Core context owned by the check-ins and access module.</summary>
/// <param name="options">Provider and connection options supplied by the host.</param>
public sealed class CheckInsDbContext(DbContextOptions<CheckInsDbContext> options)
    : DbContext(options), ICheckInsAccessUnitOfWork
{
    /// <summary>Local member and membership access projections.</summary>
    public DbSet<AccessMember> AccessMembers => Set<AccessMember>();

    /// <summary>Branches and their safe occupancy capacity.</summary>
    public DbSet<ClubBranch> Branches => Set<ClubBranch>();

    /// <summary>Member visits.</summary>
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    /// <summary>Append-only access audits.</summary>
    public DbSet<AccessAudit> AccessAudits => Set<AccessAudit>();

    /// <summary>Applies module configurations.</summary>
    /// <param name="modelBuilder">Model builder supplied by EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new AccessMemberConfiguration());
        modelBuilder.ApplyConfiguration(new ClubBranchConfiguration());
        modelBuilder.ApplyConfiguration(new CheckInConfiguration());
        modelBuilder.ApplyConfiguration(new AccessAuditConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>Stamps audit fields and persists pending changes.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of affected rows.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Kinevor.Platform.Domain.Common.Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(Kinevor.Platform.Domain.Common.Entity.CreatedAt)).CurrentValue = now;

            if (entry.State == EntityState.Modified)
                entry.Property(nameof(Kinevor.Platform.Domain.Common.Entity.UpdatedAt)).CurrentValue = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
