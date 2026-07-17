using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.ToTable("user_progress");

        // Shared-key one-to-one: UserId is both the primary key and the foreign key to User.
        builder.HasKey(p => p.UserId);

        builder.Property(p => p.Xp).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.Coins).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.CurrentStreakDays).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.LongestStreakDays).IsRequired().HasDefaultValue(0);

        builder.Property(p => p.LastActivityDateUtc).HasColumnType("date");

        builder.Property(p => p.AiTokensUsedToday).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.AiUsageResetDateUtc).HasColumnType("date");

        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<UserProgress>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
