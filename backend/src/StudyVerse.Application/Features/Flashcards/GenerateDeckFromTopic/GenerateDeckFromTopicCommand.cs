using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Flashcards.GenerateDeckFromTopic;

/// <summary>Calls <c>IFlashcardGenerationProvider</c> for <paramref name="CardCount"/> real
/// AI-generated flashcard pairs on <paramref name="Topic"/>, and creates the deck + cards in one go.</summary>
public sealed record GenerateDeckFromTopicCommand(Guid UserId, string Title, string Topic, int CardCount)
    : IRequest<Result<Guid>>;
