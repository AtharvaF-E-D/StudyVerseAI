using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class StudyPlanConfiguration : IEntityTypeConfiguration<StudyPlan>
{
    public void Configure(EntityTypeBuilder<StudyPlan> builder)
    {
        builder.ToTable("study_plans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ExamDate).IsRequired().HasColumnType("date");

        // `text`, not the default varchar - always read/written as one JSON-text blob per column,
        // same reasoning as NoteContent's *Json columns (see StudyPlan's doc comment).
        builder.Property(p => p.SubjectsJson).IsRequired().HasColumnType("text");
        builder.Property(p => p.WeakTopicsJson).IsRequired().HasColumnType("text");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc).HasColumnType("timestamptz");

        // Supports CreateStudyPlanCommandHandler's "archive this user's current active plan" lookup
        // and GetActivePlanQuery/GetTodayTasksQuery/GetWeeklyTasksQuery/MissedTaskRecoveryService's
        // "find this user's active plan". Not unique: a user may have many Archived plans over
        // time, only ever one Active one — that invariant is enforced by application logic
        // (CreateStudyPlanCommandHandler archiving the prior one), not a database constraint,
        // matching how similar single-active-row invariants are enforced elsewhere in this codebase.
        builder.HasIndex(p => new { p.UserId, p.Status });

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Deleting a plan deletes its tasks (no DeletePlanCommand in this pass, but this cascade
        // keeps orphaned task rows from ever being possible regardless).
        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Plan)
            .HasForeignKey(t => t.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
