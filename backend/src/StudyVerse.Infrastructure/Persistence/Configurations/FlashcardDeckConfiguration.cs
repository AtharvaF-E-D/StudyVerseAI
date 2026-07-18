using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class FlashcardDeckConfiguration : IEntityTypeConfiguration<FlashcardDeck>
{
    public void Configure(EntityTypeBuilder<FlashcardDeck> builder)
    {
        builder.ToTable("flashcard_decks");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title).IsRequired().HasMaxLength(200);

        builder.Property(d => d.Description).HasMaxLength(2000);

        builder.Property(d => d.ShareToken).HasMaxLength(50);

        builder.Property(d => d.CreatedAtUtc).HasColumnType("timestamptz");
        builder.Property(d => d.UpdatedAtUtc).HasColumnType("timestamptz");

        // Supports "list my decks" (GetDecksQuery).
        builder.HasIndex(d => new { d.UserId, d.CreatedAtUtc });

        // Supports the unauthenticated share-token lookup (GetSharedDeckQuery) — unique so a token
        // never resolves to more than one deck. Postgres treats NULL as distinct from any other
        // value in a unique index, so any number of un-shared decks (ShareToken == null) coexist
        // without collision.
        builder.HasIndex(d => d.ShareToken).IsUnique();

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Provenance only (see FlashcardDeck's doc comment) — deleting the source note must NOT
        // cascade-delete decks generated from it, since the deck's cards are independent rows from
        // that point on.
        builder.HasOne(d => d.SourceNote)
            .WithMany()
            .HasForeignKey(d => d.SourceNoteId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Deleting a deck deletes its cards (DeleteDeckCommand relies on this DB-level cascade).
        builder.HasMany(d => d.Cards)
            .WithOne(c => c.Deck)
            .HasForeignKey(c => c.DeckId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
