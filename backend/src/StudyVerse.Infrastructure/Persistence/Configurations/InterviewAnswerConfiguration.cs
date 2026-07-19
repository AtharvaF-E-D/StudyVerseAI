using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class InterviewAnswerConfiguration : IEntityTypeConfiguration<InterviewAnswer>
{
    public void Configure(EntityTypeBuilder<InterviewAnswer> builder)
    {
        builder.ToTable("interview_answers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AnswerText).IsRequired().HasColumnType("text");

        builder.Property(a => a.AiFeedback).HasColumnType("text");

        builder.Property(a => a.AnsweredAtUtc).HasColumnType("timestamptz");

        // Supports SubmitAnswerCommandHandler's "has this question already been answered in this
        // session" check and CompleteInterviewSessionCommandHandler's "are all 5 answered" check.
        builder.HasIndex(a => new { a.SessionId, a.QuestionId }).IsUnique();

        builder.HasOne(a => a.Session)
            .WithMany(s => s.Answers)
            .HasForeignKey(a => a.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
