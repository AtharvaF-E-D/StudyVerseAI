using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;
using StudyVerse.Infrastructure.Persistence.Seed;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class InterviewQuestionConfiguration : IEntityTypeConfiguration<InterviewQuestion>
{
    public void Configure(EntityTypeBuilder<InterviewQuestion> builder)
    {
        builder.ToTable("interview_questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(q => q.QuestionText).IsRequired().HasColumnType("text");

        builder.Property(q => q.WhatGoodAnswersCover).IsRequired().HasColumnType("text");

        builder.Property(q => q.CreatedAtUtc).HasColumnType("timestamptz");

        // Supports StartInterviewSessionCommandHandler's "5 random questions of this Type" query.
        builder.HasIndex(q => q.Type);

        builder.HasData(InterviewQuestionSeedData.All);
    }
}
