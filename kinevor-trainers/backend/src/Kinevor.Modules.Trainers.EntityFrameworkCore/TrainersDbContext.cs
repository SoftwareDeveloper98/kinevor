using Kinevor.Modules.Trainers.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.Trainers.EntityFrameworkCore;

/// <summary>EF Core context owned by the trainers module.</summary>
/// <param name="options">Provider and connection options supplied by the host.</param>
public sealed class TrainersDbContext(DbContextOptions<TrainersDbContext> options)
    : DbContext(options), IUnitOfWork
{
    /// <summary>Trainer profiles.</summary>
    public DbSet<Trainer> Trainers => Set<Trainer>();

    /// <summary>Applies the module's entity configurations.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new TrainerConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>Stamps audit fields and persists pending changes.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Persists changes, returning <see langword="false"/> on an optimistic-concurrency conflict.</summary>
    public async Task<bool> TrySaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    private void StampAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(Entity.CreatedAt)).CurrentValue = now;

            if (entry.State == EntityState.Modified)
                entry.Property(nameof(Entity.UpdatedAt)).CurrentValue = now;
        }
    }
}
