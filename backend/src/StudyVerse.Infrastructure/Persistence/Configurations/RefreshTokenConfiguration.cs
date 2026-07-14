using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        builder.HasIndex(rt => rt.UserId);

        builder.Property(rt => rt.DeviceId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rt => rt.DeviceName)
            .HasMaxLength(200);

        builder.Property(rt => rt.ReplacedByTokenHash)
            .HasMaxLength(128);

        builder.Property(rt => rt.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(rt => rt.ExpiresAtUtc).HasColumnType("timestamptz");
        builder.Property(rt => rt.RevokedAtUtc).HasColumnType("timestamptz");

        builder.Ignore(rt => rt.IsRevoked);
    }
}
