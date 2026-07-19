using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;
using StudyVerse.Infrastructure.Persistence.Seed;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class CodingProblemTestCaseConfiguration : IEntityTypeConfiguration<CodingProblemTestCase>
{
    public void Configure(EntityTypeBuilder<CodingProblemTestCase> builder)
    {
        builder.ToTable("coding_problem_test_cases");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Input).IsRequired().HasColumnType("text");
        builder.Property(t => t.ExpectedOutput).IsRequired().HasColumnType("text");

        // Supports "load a problem's test cases in a stable order" (GetProblemQueryHandler,
        // SubmitCodeCommandHandler).
        builder.HasIndex(t => new { t.ProblemId, t.OrderIndex });

        builder.HasOne(t => t.Problem)
            .WithMany(p => p.TestCases)
            .HasForeignKey(t => t.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(CodingProblemSeedData.TestCases);
    }
}
