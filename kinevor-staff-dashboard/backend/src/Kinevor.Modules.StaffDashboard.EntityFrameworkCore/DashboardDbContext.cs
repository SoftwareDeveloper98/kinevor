using Kinevor.Modules.StaffDashboard.Domain;
using Kinevor.Platform.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.StaffDashboard.EntityFrameworkCore;

/// <summary>EF Core context owned by the staff dashboard module.</summary>
public sealed class DashboardDbContext(DbContextOptions<DashboardDbContext> options) : DbContext(options)
{
    /// <summary>Current branch dashboard snapshots.</summary>
    public DbSet<DashboardSnapshot> DashboardSnapshots => Set<DashboardSnapshot>();

    /// <summary>Daily trend points owned by snapshots.</summary>
    public DbSet<DashboardTrendPoint> TrendPoints => Set<DashboardTrendPoint>();

    /// <summary>Activity feed entries owned by snapshots.</summary>
    public DbSet<DashboardActivity> Activities => Set<DashboardActivity>();

    /// <summary>Operational alerts owned by snapshots.</summary>
    public DbSet<DashboardAlert> Alerts => Set<DashboardAlert>();

    /// <summary>Applies all module entity configurations.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DashboardDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>Stamps audit fields and persists pending changes.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(Entity.CreatedAt)).CurrentValue = now;

            if (entry.State == EntityState.Modified)
                entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
