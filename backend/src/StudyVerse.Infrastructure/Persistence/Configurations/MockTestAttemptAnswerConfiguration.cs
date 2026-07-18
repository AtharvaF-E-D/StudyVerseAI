using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class MockTestAttemptAnswerConfiguration : IEntityTypeConfiguration<MockTestAttemptAnswer>
{
    public void Configure(EntityTypeBuilder<MockTestAttemptAnswer> builder)
    {
        builder.ToTable("mock_test_attempt_answers");

        builder.HasKey(a => a.Id);

        // Supports "load this attempt's questions in order" (start response + review).
        builder.HasIndex(a => new { a.AttemptId, a.OrderIndex }).IsUnique();

        builder.HasIndex(a => a.QuestionId);

        // The Attempt-side FK + cascade-delete is configured in MockTestAttemptConfiguration.HasMany.
        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
