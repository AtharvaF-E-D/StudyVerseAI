using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("user_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(t => t.TokenHash);

        builder.HasIndex(t => new { t.UserId, t.Purpose });

        builder.Property(t => t.Purpose)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(t => t.ExpiresAtUtc).HasColumnType("timestamptz");
        builder.Property(t => t.ConsumedAtUtc).HasColumnType("timestamptz");

        builder.Ignore(t => t.IsConsumed);
    }
}
