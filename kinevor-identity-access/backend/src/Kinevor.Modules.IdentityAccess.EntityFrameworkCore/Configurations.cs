using Kinevor.Modules.IdentityAccess.Domain;
using Kinevor.Platform.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kinevor.Modules.IdentityAccess.EntityFrameworkCore;

/// <summary>EF Core mapping for <see cref="UserAccount"/>.</summary>
public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    /// <summary>Configures keys, indexes and value conversions.</summary>
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("UserAccounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(account => account.Role).HasMaxLength(20).IsRequired();
        builder.Property(account => account.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(account => account.Email)
            .HasConversion(email => email.Value, value => Email.Create(value).Value)
            .HasMaxLength(200)
            .IsRequired();
        builder.HasIndex(account => account.Email).IsUnique();
        builder.Ignore(account => account.DomainEvents);
    }
}

/// <summary>EF Core mapping for <see cref="RefreshToken"/>.</summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>Configures keys and lookup indexes.</summary>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.UserId);
        builder.Ignore(token => token.DomainEvents);
    }
}
