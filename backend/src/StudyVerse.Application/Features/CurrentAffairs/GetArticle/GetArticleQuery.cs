using MediatR;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticle;

/// <summary>Single cached article detail, by our own <see cref="Domain.Entities.NewsArticle.Id"/> (not GNews's ExternalId).</summary>
public sealed record GetArticleQuery(Guid ArticleId) : IRequest<Result<NewsArticleDto>>;
