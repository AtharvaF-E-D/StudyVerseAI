using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence.Configurations;

public sealed class NewsArticleQuizConfiguration : IEntityTypeConfiguration<NewsArticleQuiz>
{
    public void Configure(EntityTypeBuilder<NewsArticleQuiz> builder)
    {
        builder.ToTable("news_article_quizzes");

        builder.HasKey(q => q.Id);

        // `text`, not the default varchar - the serialized question array.
        builder.Property(q => q.QuestionsJson).IsRequired().HasColumnType("text");

        builder.Property(q => q.GeneratedAtUtc).HasColumnType("timestamptz");

        // One cached quiz per article - GetArticleQuizQuery's cache-first check relies on this.
        builder.HasIndex(q => q.ArticleId).IsUnique();

        builder.HasOne(q => q.Article)
            .WithMany()
            .HasForeignKey(q => q.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
