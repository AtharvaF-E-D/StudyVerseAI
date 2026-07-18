using MediatR;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetBookmarks;

/// <summary>A user's bookmarked articles, most recently bookmarked first.</summary>
public sealed record GetBookmarksQuery(Guid UserId) : IRequest<Result<IReadOnlyList<NewsArticleDto>>>;
