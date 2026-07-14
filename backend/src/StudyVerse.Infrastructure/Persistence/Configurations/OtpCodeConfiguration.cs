using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("otp_codes");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Destination)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(o => o.Destination);

        builder.Property(o => o.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Purpose)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(o => o.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(o => o.ExpiresAtUtc).HasColumnType("timestamptz");
        builder.Property(o => o.ConsumedAtUtc).HasColumnType("timestamptz");

        builder.Ignore(o => o.IsConsumed);
    }
}
