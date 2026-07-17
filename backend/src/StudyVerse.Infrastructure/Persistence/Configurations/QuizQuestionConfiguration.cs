using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;
using StudyVerse.Infrastructure.Persistence.Seed;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("quiz_questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Category)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(q => q.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(q => q.QuestionText)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(q => q.OptionA).IsRequired().HasMaxLength(300);
        builder.Property(q => q.OptionB).IsRequired().HasMaxLength(300);
        builder.Property(q => q.OptionC).IsRequired().HasMaxLength(300);
        builder.Property(q => q.OptionD).IsRequired().HasMaxLength(300);

        builder.Property(q => q.Explanation)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(q => q.CreatedAtUtc).HasColumnType("timestamptz");

        // Supports "pick 10 random questions for this category+difficulty" (StartQuizSessionCommandHandler).
        builder.HasIndex(q => new { q.Category, q.Difficulty });

        builder.HasData(QuizQuestionSeedData.All);
    }
}
