using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class CodeSubmissionConfiguration : IEntityTypeConfiguration<CodeSubmission>
{
    public void Configure(EntityTypeBuilder<CodeSubmission> builder)
    {
        builder.ToTable("code_submissions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SourceCode).IsRequired().HasColumnType("text");

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.SubmittedAtUtc).HasColumnType("timestamptz");

        // Supports "has this user ever been Accepted on this problem" (SubmitCodeCommandHandler)
        // and "this user's submission history, optionally for one problem" (GetSubmissionsQuery).
        builder.HasIndex(s => new { s.UserId, s.ProblemId, s.Status });
        builder.HasIndex(s => new { s.UserId, s.SubmittedAtUtc });

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Problem)
            .WithMany()
            .HasForeignKey(s => s.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
