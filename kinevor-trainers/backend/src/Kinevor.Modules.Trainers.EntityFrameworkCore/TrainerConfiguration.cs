using Kinevor.Modules.Trainers.Domain;
using Kinevor.Platform.Domain.Enums;
using Kinevor.Platform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kinevor.Modules.Trainers.EntityFrameworkCore;

/// <summary>EF Core mapping for the <see cref="Trainer"/> aggregate.</summary>
public sealed class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    /// <summary>Configures keys, indexes, conversions and column constraints.</summary>
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Trainers");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LastName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Bio).HasMaxLength(2000).IsRequired();
        builder.Property(t => t.PhotoUrl).HasMaxLength(500);

        builder.Property(t => t.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Phone)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value).Value)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Specializations)
            .HasConversion(
                list => string.Join(",", list.Select(s => (int)s)),
                value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => (ClassType)int.Parse(item, System.Globalization.CultureInfo.InvariantCulture))
                    .ToList())
            .HasMaxLength(500);

        builder.Property(t => t.Certifications)
            .HasConversion(
                list => string.Join("|", list),
                value => value.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList())
            .HasMaxLength(2000);

        builder.Property(t => t.Rating).HasPrecision(3, 2);
        builder.HasIndex(t => t.Email).IsUnique();
        builder.Ignore(t => t.FullName);
        builder.Ignore(t => t.DomainEvents);
    }
}
