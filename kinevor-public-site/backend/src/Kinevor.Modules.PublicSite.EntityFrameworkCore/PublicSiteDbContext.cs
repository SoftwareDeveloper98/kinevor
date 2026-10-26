using Kinevor.Modules.PublicSite.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.PublicSite.EntityFrameworkCore;

/// <summary>EF Core context owned by the public-site module.</summary>
public sealed class PublicSiteDbContext(DbContextOptions<PublicSiteDbContext> options)
    : DbContext(options)
{
    /// <summary>Brand, contact and social-proof profile.</summary>
    public DbSet<SiteProfile> SiteProfiles => Set<SiteProfile>();

    /// <summary>Gym locations.</summary>
    public DbSet<Branch> Branches => Set<Branch>();

    /// <summary>Membership offers.</summary>
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();

    /// <summary>Coach profiles.</summary>
    public DbSet<Trainer> Trainers => Set<Trainer>();

    /// <summary>Scheduled group classes.</summary>
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();

    /// <summary>Frequently asked questions.</summary>
    public DbSet<FaqEntry> FaqEntries => Set<FaqEntry>();

    /// <summary>Blog articles.</summary>
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    /// <summary>Website enquiries.</summary>
    public DbSet<Lead> Leads => Set<Lead>();

    /// <summary>Free-trial requests.</summary>
    public DbSet<TrialBooking> TrialBookings => Set<TrialBooking>();

    /// <summary>Applies public-site mappings.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new SiteProfileConfiguration());
        modelBuilder.ApplyConfiguration(new BranchConfiguration());
        modelBuilder.ApplyConfiguration(new MembershipPlanConfiguration());
        modelBuilder.ApplyConfiguration(new TrainerConfiguration());
        modelBuilder.ApplyConfiguration(new ClassSessionConfiguration());
        modelBuilder.ApplyConfiguration(new FaqEntryConfiguration());
        modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
        modelBuilder.ApplyConfiguration(new LeadConfiguration());
        modelBuilder.ApplyConfiguration(new TrialBookingConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>Stamps audit fields and persists pending changes.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Stamps audit fields and persists pending changes synchronously.</summary>
    public override int SaveChanges()
    {
        StampAuditFields();
        return base.SaveChanges();
    }

    private void StampAuditFields()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Kinevor.Platform.Domain.Common.Entity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(nameof(Kinevor.Platform.Domain.Common.Entity.CreatedAt)).CurrentValue = now;

            if (entry.State == EntityState.Modified)
                entry.Property(nameof(Kinevor.Platform.Domain.Common.Entity.UpdatedAt)).CurrentValue = now;
        }
    }
}
