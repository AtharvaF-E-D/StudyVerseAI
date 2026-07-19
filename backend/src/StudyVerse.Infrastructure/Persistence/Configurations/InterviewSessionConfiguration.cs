using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class InterviewSessionConfiguration : IEntityTypeConfiguration<InterviewSession>
{
    public void Configure(EntityTypeBuilder<InterviewSession> builder)
    {
        builder.ToTable("interview_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.SelectedQuestionIdsJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(s => s.ImprovementPlan).HasColumnType("text");

        builder.Property(s => s.StartedAtUtc).HasColumnType("timestamptz");
        builder.Property(s => s.CompletedAtUtc).HasColumnType("timestamptz");

        // Supports GetInterviewSessionsQuery's history list and GetInterviewStatsQuery's aggregates.
        builder.HasIndex(s => new { s.UserId, s.StartedAtUtc });

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
