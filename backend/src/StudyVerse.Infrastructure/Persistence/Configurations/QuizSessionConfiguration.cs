using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class QuizSessionConfiguration : IEntityTypeConfiguration<QuizSession>
{
    public void Configure(EntityTypeBuilder<QuizSession> builder)
    {
        builder.ToTable("quiz_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.DailyChallengeDateUtc).HasColumnType("date");

        builder.Property(s => s.StartedAtUtc).HasColumnType("timestamptz");
        builder.Property(s => s.EndedAtUtc).HasColumnType("timestamptz");

        // Enforces "at most one daily-challenge session per user per UTC day". Postgres treats
        // NULL as distinct from any other value in a unique index, so ordinary (non-daily)
        // sessions — which always have a null DailyChallengeDateUtc — never collide with each
        // other here; no partial/filtered index is needed.
        builder.HasIndex(s => new { s.UserId, s.DailyChallengeDateUtc }).IsUnique();

        // Supports "this user's most recent completed sessions" (anti-repetition selection) and
        // resume/history lookups.
        builder.HasIndex(s => new { s.UserId, s.StartedAtUtc });

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Questions)
            .WithOne(sq => sq.Session)
            .HasForeignKey(sq => sq.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
