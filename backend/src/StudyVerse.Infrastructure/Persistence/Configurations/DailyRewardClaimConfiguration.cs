using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class DailyRewardClaimConfiguration : IEntityTypeConfiguration<DailyRewardClaim>
{
    public void Configure(EntityTypeBuilder<DailyRewardClaim> builder)
    {
        builder.ToTable("daily_reward_claims");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClaimDateUtc).HasColumnType("date").IsRequired();

        // Prevents claiming the daily reward twice on the same UTC calendar date. Also supports
        // ClaimDailyRewardCommandHandler/GetDailyRewardStatusQueryHandler's "most recent claim"
        // lookup (OrderByDescending on ClaimDateUtc).
        builder.HasIndex(c => new { c.UserId, c.ClaimDateUtc }).IsUnique();

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
