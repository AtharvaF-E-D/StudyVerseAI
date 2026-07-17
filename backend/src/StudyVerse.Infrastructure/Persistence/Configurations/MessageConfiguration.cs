using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        // `text`, not the default varchar, since assistant replies (with code/LaTeX) can be long.
        builder.Property(m => m.Content)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.CreatedAtUtc).HasColumnType("timestamptz");

        // Supports "load a conversation's messages in order" (GetConversationMessagesQuery) and
        // "find the most recent message per conversation" (GetConversationsQuery's preview).
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAtUtc });

        // The FK + cascade-delete behavior is configured on the Conversation side
        // (ConversationConfiguration.HasMany), so it isn't repeated here.
    }
}
