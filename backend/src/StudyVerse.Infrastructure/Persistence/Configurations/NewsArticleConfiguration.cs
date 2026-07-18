using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class NewsArticleConfiguration : IEntityTypeConfiguration<NewsArticle>
{
    public void Configure(EntityTypeBuilder<NewsArticle> builder)
    {
        builder.ToTable("news_articles");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ExternalId).IsRequired().HasMaxLength(200);

        builder.Property(a => a.Title).IsRequired().HasMaxLength(500);

        builder.Property(a => a.Description).HasColumnType("text");

        // `text`, not the default varchar - GNews's `content` field can run long.
        builder.Property(a => a.Content).IsRequired().HasColumnType("text");

        builder.Property(a => a.Url).IsRequired().HasMaxLength(2000);

        builder.Property(a => a.ImageUrl).HasMaxLength(2000);

        builder.Property(a => a.Category).IsRequired().HasMaxLength(50);

        builder.Property(a => a.SourceName).IsRequired().HasMaxLength(200);

        builder.Property(a => a.PublishedAtUtc).HasColumnType("timestamptz");
        builder.Property(a => a.FetchedAtUtc).HasColumnType("timestamptz");

        // GNews's own article id - unique so re-fetching a category never inserts the same story
        // twice (NewsArticleUpsertService looks existing rows up by this).
        builder.HasIndex(a => a.ExternalId).IsUnique();

        // Supports GetArticlesByCategoryQuery's two lookups: "how fresh is this category's cache"
        // (ordered by FetchedAtUtc) and "the freshest N articles for this category" (ordered by
        // PublishedAtUtc) - both filter on Category first.
        builder.HasIndex(a => new { a.Category, a.FetchedAtUtc });
        builder.HasIndex(a => new { a.Category, a.PublishedAtUtc });
    }
}
