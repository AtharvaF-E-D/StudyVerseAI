using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class NewsBookmarkConfiguration : IEntityTypeConfiguration<NewsBookmark>
{
    public void Configure(EntityTypeBuilder<NewsBookmark> builder)
    {
        builder.ToTable("news_bookmarks");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CreatedAtUtc).HasColumnType("timestamptz");

        // ToggleBookmarkCommand relies on "does a row for (UserId, ArticleId) already exist" as the
        // sole source of truth for the current bookmarked state - see NewsBookmark's doc comment.
        builder.HasIndex(b => new { b.UserId, b.ArticleId }).IsUnique();

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Article)
            .WithMany()
            .HasForeignKey(b => b.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
