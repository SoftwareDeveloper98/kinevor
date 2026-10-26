using Kinevor.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kinevor.Modules.Notifications.EntityFrameworkCore;

/// <summary>EF Core mapping for <see cref="Notification"/>.</summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    /// <summary>Configures keys, indexes, lengths and conversions.</summary>
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Notifications");
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Title).HasMaxLength(Notification.TitleMaxLength).IsRequired();
        builder.Property(notification => notification.Body).HasMaxLength(Notification.BodyMaxLength).IsRequired();
        builder.Property(notification => notification.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(notification => notification.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(notification => notification.Category).HasMaxLength(60).IsRequired();
        builder.Property(notification => notification.RecipientEmail).HasMaxLength(200);
        builder.Property(notification => notification.TemplateKey).HasMaxLength(80);
        builder.Property(notification => notification.FailureReason).HasMaxLength(500);
        builder.HasIndex(notification => new { notification.MemberId, notification.CreatedAt });
        builder.HasIndex(notification => new { notification.Status, notification.ScheduledAt });
        builder.HasIndex(notification => notification.Category);
        builder.Ignore(notification => notification.DomainEvents);
    }
}

/// <summary>EF Core mapping for <see cref="NotificationPreference"/>.</summary>
public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    /// <summary>Configures keys and the unique member/category pair.</summary>
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("NotificationPreferences");
        builder.HasKey(preference => preference.Id);
        builder.Property(preference => preference.Category).HasMaxLength(60).IsRequired();
        builder.HasIndex(preference => new { preference.MemberId, preference.Category }).IsUnique();
    }
}

/// <summary>EF Core mapping for <see cref="DeviceToken"/>.</summary>
public sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    /// <summary>Configures keys, token uniqueness and member lookup.</summary>
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("NotificationDevices");
        builder.HasKey(device => device.Id);
        builder.Property(device => device.Token).HasMaxLength(500).IsRequired();
        builder.Property(device => device.Platform).HasMaxLength(20).IsRequired();
        builder.HasIndex(device => device.Token).IsUnique();
        builder.HasIndex(device => device.MemberId);
    }
}

/// <summary>EF Core mapping for <see cref="EmailSuppression"/>.</summary>
public sealed class EmailSuppressionConfiguration : IEntityTypeConfiguration<EmailSuppression>
{
    /// <summary>Configures keys and address uniqueness.</summary>
    public void Configure(EntityTypeBuilder<EmailSuppression> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("EmailSuppressions");
        builder.HasKey(suppression => suppression.Id);
        builder.Property(suppression => suppression.Email).HasMaxLength(200).IsRequired();
        builder.Property(suppression => suppression.Reason).HasMaxLength(30).IsRequired();
        builder.Property(suppression => suppression.Detail).HasMaxLength(500);
        builder.HasIndex(suppression => suppression.Email).IsUnique();
    }
}

/// <summary>EF Core mapping for <see cref="NotificationTemplate"/>.</summary>
public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    /// <summary>Configures keys and template key uniqueness.</summary>
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("NotificationTemplates");
        builder.HasKey(template => template.Id);
        builder.Property(template => template.Key).HasMaxLength(80).IsRequired();
        builder.Property(template => template.Name).HasMaxLength(160).IsRequired();
        builder.Property(template => template.Category).HasMaxLength(60).IsRequired();
        builder.Property(template => template.DefaultChannel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(template => template.TitleTemplate).HasMaxLength(300).IsRequired();
        builder.Property(template => template.BodyTemplate).HasMaxLength(4000).IsRequired();
        builder.HasIndex(template => template.Key).IsUnique();
    }
}
