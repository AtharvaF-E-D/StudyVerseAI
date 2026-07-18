using MediatR;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GetDueCards;

/// <summary>Today's review queue: cards with <c>NextReviewDateUtc &lt;= today</c>, across every
/// deck the user owns, or scoped to a single deck when <paramref name="DeckId"/> is given.</summary>
public sealed record GetDueCardsQuery(Guid UserId, Guid? DeckId) : IRequest<Result<IReadOnlyList<DueCardDto>>>;
