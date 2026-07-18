using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class StudyPlanTaskConfiguration : IEntityTypeConfiguration<StudyPlanTask>
{
    public void Configure(EntityTypeBuilder<StudyPlanTask> builder)
    {
        builder.ToTable("study_plan_tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ScheduledDateUtc).IsRequired().HasColumnType("date");
        builder.Property(t => t.OriginalScheduledDateUtc).HasColumnType("date");

        builder.Property(t => t.Subject).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Topic).IsRequired().HasMaxLength(500);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.CompletedAtUtc).HasColumnType("timestamptz");

        // Supports GetTodayTasksQuery ("this plan's tasks scheduled today"), GetWeeklyTasksQuery
        // ("this plan's tasks in a date range"), and MissedTaskRecoveryService's "this plan's still-
        // Pending tasks scheduled before today" / "this plan's future days' committed minutes".
        builder.HasIndex(t => new { t.PlanId, t.ScheduledDateUtc });

        // The FK + cascade-delete is configured on the StudyPlan side (StudyPlanConfiguration's
        // HasMany(p => p.Tasks)), so it isn't repeated here.
    }
}
