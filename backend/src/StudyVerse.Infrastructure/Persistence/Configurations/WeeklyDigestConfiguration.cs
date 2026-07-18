using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class WeeklyDigestConfiguration : IEntityTypeConfiguration<WeeklyDigest>
{
    public void Configure(EntityTypeBuilder<WeeklyDigest> builder)
    {
        builder.ToTable("weekly_digests");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.WeekStartDateUtc).HasColumnType("date");

        // `text`, not the default varchar - a 3-4 paragraph AI-generated digest.
        builder.Property(d => d.SummaryText).IsRequired().HasColumnType("text");

        builder.Property(d => d.GeneratedAtUtc).HasColumnType("timestamptz");

        // One digest per ISO week, shared across all users - see WeeklyDigest's doc comment.
        builder.HasIndex(d => d.WeekStartDateUtc).IsUnique();
    }
}
