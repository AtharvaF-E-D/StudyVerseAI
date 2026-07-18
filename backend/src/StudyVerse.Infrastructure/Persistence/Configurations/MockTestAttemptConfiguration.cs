using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class MockTestAttemptConfiguration : IEntityTypeConfiguration<MockTestAttempt>
{
    public void Configure(EntityTypeBuilder<MockTestAttempt> builder)
    {
        builder.ToTable("mock_test_attempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.AiWeaknessAnalysis).HasColumnType("text");

        builder.Property(a => a.StartedAtUtc).HasColumnType("timestamptz");
        builder.Property(a => a.SubmittedAtUtc).HasColumnType("timestamptz");

        // Supports "this user's past mock test attempts" (GetMockTestAttemptsQuery).
        builder.HasIndex(a => new { a.UserId, a.StartedAtUtc });

        // Supports the percentile-rank computation: "every other Submitted attempt for this template".
        builder.HasIndex(a => new { a.TemplateId, a.Status });

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Answers)
            .WithOne(ans => ans.Attempt)
            .HasForeignKey(ans => ans.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
