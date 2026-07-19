using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class ResumeAnalysisConfiguration : IEntityTypeConfiguration<ResumeAnalysis>
{
    public void Configure(EntityTypeBuilder<ResumeAnalysis> builder)
    {
        builder.ToTable("resume_analyses");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.FileName).IsRequired().HasMaxLength(260);

        builder.Property(r => r.StoredFilePath).IsRequired().HasMaxLength(500);

        builder.Property(r => r.StrengthsJson).IsRequired().HasColumnType("jsonb");
        builder.Property(r => r.WeaknessesJson).IsRequired().HasColumnType("jsonb");
        builder.Property(r => r.SuggestionsJson).IsRequired().HasColumnType("jsonb");

        builder.Property(r => r.AnalyzedAtUtc).HasColumnType("timestamptz");

        // Supports GetResumeAnalysesQuery's history list and GetInterviewStatsQuery's count.
        builder.HasIndex(r => new { r.UserId, r.AnalyzedAtUtc });

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
