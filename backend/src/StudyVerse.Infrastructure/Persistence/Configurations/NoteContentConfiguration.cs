using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class NoteContentConfiguration : IEntityTypeConfiguration<NoteContent>
{
    public void Configure(EntityTypeBuilder<NoteContent> builder)
    {
        builder.ToTable("note_contents");

        builder.HasKey(c => c.Id);

        // All `text`: generated content (revision sheets, mcq sets, etc.) can be long, and these
        // are always read/written as one JSON-text blob per column — see NoteContent's doc comment
        // for why that's fine here rather than normalizing into relational sub-tables.
        builder.Property(c => c.Summary).IsRequired().HasColumnType("text");
        builder.Property(c => c.KeyPointsJson).IsRequired().HasColumnType("text");
        builder.Property(c => c.FlashcardsJson).IsRequired().HasColumnType("text");
        builder.Property(c => c.McqsJson).IsRequired().HasColumnType("text");
        builder.Property(c => c.MindMapJson).IsRequired().HasColumnType("text");
        builder.Property(c => c.RevisionSheet).IsRequired().HasColumnType("text");
        builder.Property(c => c.VocabularyJson).IsRequired().HasColumnType("text");
        builder.Property(c => c.FormulasJson).IsRequired().HasColumnType("text");

        // The FK + cascade-delete behavior is configured on the Note side (NoteConfiguration's
        // HasOne(n => n.Content)), so it isn't repeated here — this index just enforces the 1:1.
        builder.HasIndex(c => c.NoteId).IsUnique();
    }
}
