using System.Text.Json;
using Kinevor.Modules.PublicSite.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kinevor.Modules.PublicSite.EntityFrameworkCore;

internal static class StringListMapping
{
    public static void Map(
        PropertyBuilder<IReadOnlyList<string>> property,
        string columnType = "TEXT")
    {
        var converter = new ValueConverter<IReadOnlyList<string>, string>(
            list => JsonSerializer.Serialize(list, JsonSerializerOptions.Default),
            json => JsonSerializer.Deserialize<List<string>>(json, JsonSerializerOptions.Default) ?? new List<string>());
        var comparer = new ValueComparer<IReadOnlyList<string>>(
            (left, right) => left != null && right != null && left.SequenceEqual(right),
            list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            list => list.ToArray());

        property.HasColumnType(columnType).HasConversion(converter, comparer);
    }
}

/// <summary>Mapping for <see cref="SiteProfile"/>.</summary>
public sealed class SiteProfileConfiguration : IEntityTypeConfiguration<SiteProfile>
{
    /// <summary>Configures table, lengths and JSON-backed highlights.</summary>
    public void Configure(EntityTypeBuilder<SiteProfile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("SiteProfiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Name).HasMaxLength(120).IsRequired();
        builder.Property(profile => profile.Tagline).HasMaxLength(160).IsRequired();
        builder.Property(profile => profile.Description).HasMaxLength(600).IsRequired();
        builder.Property(profile => profile.Story).HasMaxLength(2000).IsRequired();
        builder.Property(profile => profile.Eyebrow).HasMaxLength(120).IsRequired();
        builder.Property(profile => profile.Phone).HasMaxLength(40).IsRequired();
        builder.Property(profile => profile.Email).HasMaxLength(200).IsRequired();
        builder.Property(profile => profile.Address).HasMaxLength(240).IsRequired();
        builder.Property(profile => profile.City).HasMaxLength(120).IsRequired();
        builder.Property(profile => profile.InstagramHandle).HasMaxLength(80).IsRequired();
        builder.Property(profile => profile.Rating).HasPrecision(3, 1);
        StringListMapping.Map(builder.Property(profile => profile.Highlights));
        builder.Ignore(profile => profile.DomainEvents);
    }
}

/// <summary>Mapping for <see cref="Branch"/>.</summary>
public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    /// <summary>Configures table, unique slugs and JSON-backed amenities.</summary>
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Branches");
        builder.HasKey(branch => branch.Id);
        builder.Property(branch => branch.Name).HasMaxLength(160).IsRequired();
        builder.Property(branch => branch.Slug).HasMaxLength(120).IsRequired();
        builder.Property(branch => branch.Neighbourhood).HasMaxLength(120).IsRequired();
        builder.Property(branch => branch.Address).HasMaxLength(240).IsRequired();
        builder.Property(branch => branch.City).HasMaxLength(120).IsRequired();
        builder.Property(branch => branch.Phone).HasMaxLength(40).IsRequired();
        builder.Property(branch => branch.OpeningHours).HasMaxLength(240).IsRequired();
        builder.Property(branch => branch.Parking).HasMaxLength(500).IsRequired();
        builder.Property(branch => branch.Theme).HasMaxLength(30).IsRequired();
        StringListMapping.Map(builder.Property(branch => branch.Amenities));
        builder.HasIndex(branch => branch.Slug).IsUnique();
        builder.HasIndex(branch => new { branch.IsPublished, branch.SortOrder });
        builder.Ignore(branch => branch.DomainEvents);
    }
}

/// <summary>Mapping for <see cref="MembershipPlan"/>.</summary>
public sealed class MembershipPlanConfiguration : IEntityTypeConfiguration<MembershipPlan>
{
    /// <summary>Configures table, pricing precision and feature storage.</summary>
    public void Configure(EntityTypeBuilder<MembershipPlan> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("MembershipPlans");
        builder.HasKey(plan => plan.Id);
        builder.Property(plan => plan.Name).HasMaxLength(160).IsRequired();
        builder.Property(plan => plan.Slug).HasMaxLength(120).IsRequired();
        builder.Property(plan => plan.Description).HasMaxLength(600).IsRequired();
        builder.Property(plan => plan.Tier).HasMaxLength(60).IsRequired();
        builder.Property(plan => plan.Price).HasPrecision(12, 2);
        builder.Property(plan => plan.Currency).HasMaxLength(3).IsRequired();
        builder.Property(plan => plan.Badge).HasMaxLength(80);
        StringListMapping.Map(builder.Property(plan => plan.Features));
        builder.HasIndex(plan => plan.Slug).IsUnique();
        builder.HasIndex(plan => new { plan.IsPublished, plan.SortOrder });
        builder.Ignore(plan => plan.DomainEvents);
    }
}

/// <summary>Mapping for <see cref="Trainer"/>.</summary>
public sealed class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    /// <summary>Configures table, unique slugs and JSON-backed lists.</summary>
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Trainers");
        builder.HasKey(trainer => trainer.Id);
        builder.Property(trainer => trainer.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(trainer => trainer.LastName).HasMaxLength(80).IsRequired();
        builder.Property(trainer => trainer.Slug).HasMaxLength(140).IsRequired();
        builder.Property(trainer => trainer.RoleTitle).HasMaxLength(160).IsRequired();
        builder.Property(trainer => trainer.Bio).HasMaxLength(1200).IsRequired();
        builder.Property(trainer => trainer.PhotoUrl).HasMaxLength(500);
        builder.Property(trainer => trainer.BranchName).HasMaxLength(160);
        StringListMapping.Map(builder.Property(trainer => trainer.Specializations));
        StringListMapping.Map(builder.Property(trainer => trainer.Certifications));
        builder.HasIndex(trainer => trainer.Slug).IsUnique();
        builder.HasIndex(trainer => new { trainer.IsPublished, trainer.SortOrder });
        builder.Ignore(trainer => trainer.FullName);
        builder.Ignore(trainer => trainer.DomainEvents);
    }
}

/// <summary>Mapping for <see cref="ClassSession"/>.</summary>
public sealed class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    /// <summary>Configures table, schedule indexes and lengths.</summary>
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ClassSessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.Name).HasMaxLength(160).IsRequired();
        builder.Property(session => session.Description).HasMaxLength(600).IsRequired();
        builder.Property(session => session.Category).HasMaxLength(80).IsRequired();
        builder.Property(session => session.Intensity).HasMaxLength(40).IsRequired();
        builder.Property(session => session.TrainerName).HasMaxLength(160).IsRequired();
        builder.Property(session => session.Room).HasMaxLength(120).IsRequired();
        builder.Property(session => session.BranchName).HasMaxLength(160).IsRequired();
        builder.HasIndex(session => new { session.IsPublished, session.StartsAt });
        builder.HasIndex(session => session.BranchId);
        builder.Ignore(session => session.DomainEvents);
    }
}

/// <summary>Mapping for <see cref="FaqEntry"/>.</summary>
public sealed class FaqEntryConfiguration : IEntityTypeConfiguration<FaqEntry>
{
    /// <summary>Configures table, lengths and ordering index.</summary>
    public void Configure(EntityTypeBuilder<FaqEntry> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("FaqEntries");
        builder.HasKey(faq => faq.Id);
        builder.Property(faq => faq.Category).HasMaxLength(80).IsRequired();
        builder.Property(faq => faq.Question).HasMaxLength(300).IsRequired();
        builder.Property(faq => faq.Answer).HasMaxLength(2000).IsRequired();
        builder.HasIndex(faq => new { faq.IsPublished, faq.SortOrder });
        builder.Ignore(faq => faq.DomainEvents);
    }
}

/// <summary>Mapping for <see cref="BlogPost"/>.</summary>
public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    /// <summary>Configures table, unique slugs and publication index.</summary>
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("BlogPosts");
        builder.HasKey(post => post.Id);
        builder.Property(post => post.Slug).HasMaxLength(160).IsRequired();
        builder.Property(post => post.Title).HasMaxLength(240).IsRequired();
        builder.Property(post => post.Excerpt).HasMaxLength(600).IsRequired();
        builder.Property(post => post.Content).HasMaxLength(20000).IsRequired();
        builder.Property(post => post.Author).HasMaxLength(120).IsRequired();
        builder.Property(post => post.ImageUrl).HasMaxLength(500);
        builder.HasIndex(post => post.Slug).IsUnique();
        builder.HasIndex(post => new { post.IsPublished, post.PublishedAt });
        builder.Ignore(post => post.DomainEvents);
    }
}

/// <summary>Mapping for <see cref="Lead"/>.</summary>
public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    /// <summary>Configures table, follow-up status and lookup indexes.</summary>
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Leads");
        builder.HasKey(lead => lead.Id);
        builder.Property(lead => lead.Name).HasMaxLength(160).IsRequired();
        builder.Property(lead => lead.Email).HasMaxLength(200).IsRequired();
        builder.Property(lead => lead.Phone).HasMaxLength(40);
        builder.Property(lead => lead.Interest).HasMaxLength(120).IsRequired();
        builder.Property(lead => lead.Message).HasMaxLength(2000).IsRequired();
        builder.Property(lead => lead.Source).HasMaxLength(60).IsRequired();
        builder.Property(lead => lead.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(lead => new { lead.Status, lead.CreatedAt });
        builder.HasIndex(lead => lead.Email);
        builder.Ignore(lead => lead.DomainEvents);
    }
}

/// <summary>Mapping for <see cref="TrialBooking"/>.</summary>
public sealed class TrialBookingConfiguration : IEntityTypeConfiguration<TrialBooking>
{
    /// <summary>Configures table, follow-up status and duplicate-check indexes.</summary>
    public void Configure(EntityTypeBuilder<TrialBooking> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("TrialBookings");
        builder.HasKey(trial => trial.Id);
        builder.Property(trial => trial.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(trial => trial.LastName).HasMaxLength(80).IsRequired();
        builder.Property(trial => trial.Email).HasMaxLength(200).IsRequired();
        builder.Property(trial => trial.Phone).HasMaxLength(40).IsRequired();
        builder.Property(trial => trial.ClassName).HasMaxLength(160);
        builder.Property(trial => trial.BranchName).HasMaxLength(160);
        builder.Property(trial => trial.Goal).HasMaxLength(500);
        builder.Property(trial => trial.Notes).HasMaxLength(1000);
        builder.Property(trial => trial.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(trial => new { trial.Status, trial.CreatedAt });
        builder.HasIndex(trial => new { trial.Email, trial.PreferredDate });
        builder.Ignore(trial => trial.FullName);
        builder.Ignore(trial => trial.DomainEvents);
    }
}
