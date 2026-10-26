using Kinevor.Modules.Notifications.Application;
using Kinevor.Modules.Notifications.Domain;
using Kinevor.Platform.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.Notifications.EntityFrameworkCore;

/// <summary>EF Core context owned by the notifications module.</summary>
/// <param name="options">Provider and connection options supplied by the host.</param>
public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options), INotificationsUnitOfWork
{
    /// <summary>Notification records.</summary>
    public DbSet<Notification> Notifications => Set<Notification>();

    /// <summary>Per-member category preferences.</summary>
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    /// <summary>Registered push devices.</summary>
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    /// <summary>Suppressed e-mail addresses.</summary>
    public DbSet<EmailSuppression> EmailSuppressions => Set<EmailSuppression>();

    /// <summary>Reusable message templates.</summary>
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    /// <summary>Applies module configurations.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceTokenConfiguration());
        modelBuilder.ApplyConfiguration(new EmailSuppressionConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationTemplateConfiguration());
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
