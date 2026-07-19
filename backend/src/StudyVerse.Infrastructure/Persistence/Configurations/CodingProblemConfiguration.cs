using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;
using StudyVerse.Infrastructure.Persistence.Seed;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class CodingProblemConfiguration : IEntityTypeConfiguration<CodingProblem>
{
    public void Configure(EntityTypeBuilder<CodingProblem> builder)
    {
        builder.ToTable("coding_problems");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);

        builder.Property(p => p.Description).IsRequired().HasColumnType("text");

        builder.Property(p => p.Difficulty)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Category).IsRequired().HasMaxLength(50);

        builder.Property(p => p.StarterCodeJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(p => p.CreatedAtUtc).HasColumnType("timestamptz");

        // Supports GetProblemsQuery's difficulty/category/interview-only filtering.
        builder.HasIndex(p => new { p.Difficulty, p.Category });

        builder.HasData(CodingProblemSeedData.Problems);
    }
}
