using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class UserMissionProgressConfiguration : IEntityTypeConfiguration<UserMissionProgress>
{
    public void Configure(EntityTypeBuilder<UserMissionProgress> builder)
    {
        builder.ToTable("user_mission_progresses");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.WeekStartDateUtc).HasColumnType("date").IsRequired();

        // One progress row per user, per mission template, per week.
        builder.HasIndex(p => new { p.UserId, p.MissionTemplateId, p.WeekStartDateUtc }).IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
