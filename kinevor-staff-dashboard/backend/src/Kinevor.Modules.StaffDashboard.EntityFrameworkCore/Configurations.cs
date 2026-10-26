using Kinevor.Modules.StaffDashboard.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kinevor.Modules.StaffDashboard.EntityFrameworkCore;

internal sealed class DashboardSnapshotConfiguration : IEntityTypeConfiguration<DashboardSnapshot>
{
    public void Configure(EntityTypeBuilder<DashboardSnapshot> builder)
    {
        builder.ToTable("DashboardSnapshots");
        builder.HasKey(snapshot => snapshot.Id);
        builder.HasIndex(snapshot => snapshot.BranchId).IsUnique();
        builder.Property(snapshot => snapshot.BranchName).HasMaxLength(120).IsRequired();
        builder.Property(snapshot => snapshot.BranchCity).HasMaxLength(80).IsRequired();
        builder.Property(snapshot => snapshot.CapturedOnUtc).IsRequired();
        builder.Property(snapshot => snapshot.RevenueMonth).HasPrecision(18, 2);
        builder.Property(snapshot => snapshot.RevenuePreviousMonth).HasPrecision(18, 2);
        builder.Property(snapshot => snapshot.OutstandingAmount).HasPrecision(18, 2);
        builder.Ignore(snapshot => snapshot.OccupancyPercent);

        builder.HasMany(snapshot => snapshot.TrendPoints)
            .WithOne()
            .HasForeignKey(point => point.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(snapshot => snapshot.TrendPoints).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(snapshot => snapshot.Activities)
            .WithOne()
            .HasForeignKey(activity => activity.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(snapshot => snapshot.Activities).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(snapshot => snapshot.Alerts)
            .WithOne()
            .HasForeignKey(alert => alert.SnapshotId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(snapshot => snapshot.Alerts).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class DashboardTrendPointConfiguration : IEntityTypeConfiguration<DashboardTrendPoint>
{
    public void Configure(EntityTypeBuilder<DashboardTrendPoint> builder)
    {
        builder.ToTable("DashboardTrendPoints");
        builder.HasKey(point => point.Id);
        builder.HasIndex(point => new { point.SnapshotId, point.DateUtc }).IsUnique();
        builder.Property(point => point.DateUtc).IsRequired();
        builder.Property(point => point.Revenue).HasPrecision(18, 2);
    }
}

internal sealed class DashboardActivityConfiguration : IEntityTypeConfiguration<DashboardActivity>
{
    public void Configure(EntityTypeBuilder<DashboardActivity> builder)
    {
        builder.ToTable("DashboardActivities");
        builder.HasKey(activity => activity.Id);
        builder.HasIndex(activity => new { activity.SnapshotId, activity.OccurredAtUtc });
        builder.Property(activity => activity.Kind).HasConversion<string>().HasMaxLength(32);
        builder.Property(activity => activity.Title).HasMaxLength(160).IsRequired();
        builder.Property(activity => activity.Detail).HasMaxLength(320).IsRequired();
    }
}

internal sealed class DashboardAlertConfiguration : IEntityTypeConfiguration<DashboardAlert>
{
    public void Configure(EntityTypeBuilder<DashboardAlert> builder)
    {
        builder.ToTable("DashboardAlerts");
        builder.HasKey(alert => alert.Id);
        builder.HasIndex(alert => new { alert.SnapshotId, alert.Severity });
        builder.Property(alert => alert.Type).HasConversion<string>().HasMaxLength(40);
        builder.Property(alert => alert.Severity).HasConversion<string>().HasMaxLength(16);
        builder.Property(alert => alert.Title).HasMaxLength(160).IsRequired();
        builder.Property(alert => alert.Detail).HasMaxLength(320).IsRequired();
        builder.Property(alert => alert.Reference).HasMaxLength(80).IsRequired();
    }
}
