using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class QuizSessionQuestionConfiguration : IEntityTypeConfiguration<QuizSessionQuestion>
{
    public void Configure(EntityTypeBuilder<QuizSessionQuestion> builder)
    {
        builder.ToTable("quiz_session_questions");

        builder.HasKey(sq => sq.Id);

        builder.Property(sq => sq.AnsweredAtUtc).HasColumnType("timestamptz");

        // Supports "load this session's questions in order" (current-question lookup + review).
        builder.HasIndex(sq => new { sq.SessionId, sq.OrderIndex }).IsUnique();

        // Supports the anti-repetition join ("which questions has this user recently been shown").
        builder.HasIndex(sq => sq.QuestionId);

        // The Session-side FK + cascade-delete is configured in QuizSessionConfiguration.HasMany.
        builder.HasOne(sq => sq.Question)
            .WithMany()
            .HasForeignKey(sq => sq.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
