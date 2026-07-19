using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class SpinResultConfiguration : IEntityTypeConfiguration<SpinResult>
{
    public void Configure(EntityTypeBuilder<SpinResult> builder)
    {
        builder.ToTable("spin_results");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SpinDateUtc).HasColumnType("date").IsRequired();
        builder.Property(s => s.SpunAtUtc).HasColumnType("timestamptz").IsRequired();
        builder.Property(s => s.PrizeLabel).IsRequired().HasMaxLength(100);

        // Prevents spinning twice on the same UTC calendar date.
        builder.HasIndex(s => new { s.UserId, s.SpinDateUtc }).IsUnique();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
