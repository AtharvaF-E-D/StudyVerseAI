using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.IsBookmarked).IsRequired().HasDefaultValue(false);

        builder.Property(c => c.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(c => c.UpdatedAtUtc).HasColumnType("timestamptz");

        // Supports both "list my conversations" and "sort by most recently active".
        builder.HasIndex(c => new { c.UserId, c.UpdatedAtUtc });

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Deleting a conversation deletes its messages too (DeleteConversationCommand relies on
        // this DB-level cascade rather than loading and removing each Message explicitly).
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
