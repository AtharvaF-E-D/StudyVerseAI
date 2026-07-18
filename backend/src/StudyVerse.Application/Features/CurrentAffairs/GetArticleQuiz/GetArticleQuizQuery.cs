using MediatR;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.CurrentAffairs.GetArticleQuiz;

/// <summary>Cache-first: a <c>NewsArticleQuiz</c> is generated via one <c>IAiChatProvider</c> call the
/// first time an article's quiz is requested, then reused on every later request for the same
/// article - see the handler's doc comment.</summary>
public sealed record GetArticleQuizQuery(Guid ArticleId) : IRequest<Result<NewsArticleQuizDto>>;
