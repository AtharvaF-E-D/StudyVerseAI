using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.ToTable("flashcards");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FrontText).IsRequired().HasColumnType("text");
        builder.Property(c => c.BackText).IsRequired().HasColumnType("text");

        builder.Property(c => c.ImageUrl).HasMaxLength(2000);

        builder.Property(c => c.EaseFactor).HasColumnType("double precision");

        builder.Property(c => c.NextReviewDateUtc).HasColumnType("date");
        builder.Property(c => c.LastReviewedAtUtc).HasColumnType("timestamptz");

        // Supports GetDueCardsQuery: "this user's (optionally one deck's) cards due today",
        // which joins through DeckId to filter by user - this index covers the per-deck half of
        // that filter plus the date-range predicate applied after the join.
        builder.HasIndex(c => new { c.DeckId, c.NextReviewDateUtc });

        // The cascade-delete behavior (deleting a deck deletes its cards) is configured on the
        // FlashcardDeck side (FlashcardDeckConfiguration's HasMany(d => d.Cards)), not repeated here.
    }
}
