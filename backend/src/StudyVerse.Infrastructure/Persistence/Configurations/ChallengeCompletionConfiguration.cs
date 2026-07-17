using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class ChallengeCompletionConfiguration : IEntityTypeConfiguration<ChallengeCompletion>
{
    public void Configure(EntityTypeBuilder<ChallengeCompletion> builder)
    {
        builder.ToTable("challenge_completions");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompletedDateUtc)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(c => c.CompletedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        // Prevents completing the same challenge template twice on the same day.
        builder.HasIndex(c => new { c.UserId, c.ChallengeTemplateId, c.CompletedDateUtc })
            .IsUnique();

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
