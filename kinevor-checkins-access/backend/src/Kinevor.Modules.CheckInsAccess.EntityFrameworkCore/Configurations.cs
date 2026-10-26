using Kinevor.Modules.CheckInsAccess.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kinevor.Modules.CheckInsAccess.EntityFrameworkCore;

/// <summary>EF Core mapping for <see cref="AccessMember"/>.</summary>
public sealed class AccessMemberConfiguration : IEntityTypeConfiguration<AccessMember>
{
    /// <summary>Configures keys, indexes and string conversions.</summary>
    /// <param name="builder">Entity builder.</param>
    public void Configure(EntityTypeBuilder<AccessMember> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("AccessMembers");
        builder.HasKey(member => member.Id);
        builder.Property(member => member.Id).ValueGeneratedNever();
        builder.Property(member => member.MemberNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(member => member.MemberNumber).IsUnique();
        builder.Property(member => member.FullName).HasMaxLength(160).IsRequired();
        builder.Property(member => member.Email).HasMaxLength(200);
        builder.HasIndex(member => member.Email);
        builder.Property(member => member.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(member => member.MembershipStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(member => member.MembershipPlan).HasMaxLength(100).IsRequired();
        builder.HasIndex(member => member.HomeBranchId);
        builder.HasIndex(member => member.Status);
        builder.Ignore(member => member.DomainEvents);
    }
}

/// <summary>EF Core mapping for <see cref="ClubBranch"/>.</summary>
public sealed class ClubBranchConfiguration : IEntityTypeConfiguration<ClubBranch>
{
    /// <summary>Configures keys and unique branch codes.</summary>
    /// <param name="builder">Entity builder.</param>
    public void Configure(EntityTypeBuilder<ClubBranch> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Branches");
        builder.HasKey(branch => branch.Id);
        builder.Property(branch => branch.Id).ValueGeneratedNever();
        builder.Property(branch => branch.Name).HasMaxLength(100).IsRequired();
        builder.Property(branch => branch.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(branch => branch.Code).IsUnique();
        builder.Property(branch => branch.City).HasMaxLength(100).IsRequired();
        builder.Ignore(branch => branch.DomainEvents);
    }
}

/// <summary>EF Core mapping for <see cref="CheckIn"/>.</summary>
public sealed class CheckInConfiguration : IEntityTypeConfiguration<CheckIn>
{
    /// <summary>Configures visit snapshots, indexes and the one-open-visit uniqueness rule.</summary>
    /// <param name="builder">Entity builder.</param>
    public void Configure(EntityTypeBuilder<CheckIn> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("CheckIns");
        builder.HasKey(checkIn => checkIn.Id);
        builder.Property(checkIn => checkIn.Id).ValueGeneratedNever();
        builder.Property(checkIn => checkIn.MemberName).HasMaxLength(160).IsRequired();
        builder.Property(checkIn => checkIn.MemberNumber).HasMaxLength(30).IsRequired();
        builder.Property(checkIn => checkIn.BranchName).HasMaxLength(100).IsRequired();
        builder.Property(checkIn => checkIn.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(checkIn => checkIn.Notes).HasMaxLength(500);
        builder.Property(checkIn => checkIn.RecordedBy).HasMaxLength(200).IsRequired();
        builder.Property(checkIn => checkIn.RecordedByRole).HasMaxLength(40).IsRequired();
        builder.Property(checkIn => checkIn.AccessCode).HasMaxLength(80).IsRequired();
        builder.Property(checkIn => checkIn.AccessMessage).HasMaxLength(500).IsRequired();
        builder.Ignore(checkIn => checkIn.IsOpen);
        builder.Ignore(checkIn => checkIn.DomainEvents);
        builder.HasIndex(checkIn => new { checkIn.MemberId, checkIn.CheckedInAt });
        builder.HasIndex(checkIn => new { checkIn.BranchId, checkIn.CheckedInAt });
        builder.HasIndex(checkIn => checkIn.CheckedOutAt);
        builder.HasIndex(checkIn => checkIn.MemberId)
            .IsUnique()
            .HasDatabaseName("IX_CheckIns_MemberId_Open")
            .HasFilter("\"CheckedOutAt\" IS NULL");
    }
}

/// <summary>EF Core mapping for <see cref="AccessAudit"/>.</summary>
public sealed class AccessAuditConfiguration : IEntityTypeConfiguration<AccessAudit>
{
    /// <summary>Configures audit snapshots and query indexes.</summary>
    /// <param name="builder">Entity builder.</param>
    public void Configure(EntityTypeBuilder<AccessAudit> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("AccessAudits");
        builder.HasKey(audit => audit.Id);
        builder.Property(audit => audit.Id).ValueGeneratedNever();
        builder.Property(audit => audit.MemberNumber).HasMaxLength(30).IsRequired();
        builder.Property(audit => audit.MemberName).HasMaxLength(160).IsRequired();
        builder.Property(audit => audit.Action).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(audit => audit.Outcome).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(audit => audit.Code).HasMaxLength(80).IsRequired();
        builder.Property(audit => audit.Message).HasMaxLength(500).IsRequired();
        builder.Property(audit => audit.ActorId).HasMaxLength(200).IsRequired();
        builder.Property(audit => audit.ActorRole).HasMaxLength(40).IsRequired();
        builder.Property(audit => audit.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(audit => new { audit.MemberId, audit.OccurredAt });
        builder.HasIndex(audit => new { audit.BranchId, audit.OccurredAt });
        builder.HasIndex(audit => audit.OccurredAt);
    }
}
