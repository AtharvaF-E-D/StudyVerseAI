using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.CurrentAffairs.Common;

public static class NewsArticleMappings
{
    public static NewsArticleDto ToDto(NewsArticle article) => new(
        article.Id,
        article.Title,
        article.Description,
        article.Content,
        article.Url,
        article.ImageUrl,
        article.Category,
        article.SourceName,
        article.PublishedAtUtc);
}
