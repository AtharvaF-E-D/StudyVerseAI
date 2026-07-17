using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("notes");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);

        builder.Property(n => n.SourceFileName).IsRequired().HasMaxLength(260);

        builder.Property(n => n.SourceFileType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.StorageKey).IsRequired().HasMaxLength(500);

        // `text`, not the default varchar, since extracted document text can be long.
        builder.Property(n => n.ExtractedText).IsRequired().HasColumnType("text");

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.ErrorMessage).HasMaxLength(2000);

        builder.Property(n => n.CreatedAtUtc).HasColumnType("timestamptz");

        // Supports "list my notes, most recent first" (GetNotesQuery).
        builder.HasIndex(n => new { n.UserId, n.CreatedAtUtc });

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Deleting a note deletes its generated content too (DeleteNoteCommand relies on this
        // DB-level cascade rather than removing NoteContent explicitly) — the stored file itself
        // is removed separately via IFileStorageService.DeleteAsync, since that's not the
        // database's concern.
        builder.HasOne(n => n.Content)
            .WithOne(c => c.Note)
            .HasForeignKey<NoteContent>(c => c.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
