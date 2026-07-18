using MediatR;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Features.Flashcards.GenerateDeckFromTopic;

public sealed class GenerateDeckFromTopicCommandHandler : IRequestHandler<GenerateDeckFromTopicCommand, Result<Guid>>
{
    private readonly IAppDbContext _db;
    private readonly IFlashcardGenerationProvider _flashcardGenerationProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GenerateDeckFromTopicCommandHandler(
        IAppDbContext db,
        IFlashcardGenerationProvider flashcardGenerationProvider,
        IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _flashcardGenerationProvider = flashcardGenerationProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(GenerateDeckFromTopicCommand request, CancellationToken cancellationToken)
    {
        var generatedCards = await _flashcardGenerationProvider.GenerateFlashcardsAsync(
            request.Topic, request.CardCount, cancellationToken);

        if (generatedCards.Count == 0)
        {
            return Result.Failure<Guid>("The AI didn't return any flashcards for that topic. Please try again.");
        }

        var now = _dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            Description = $"AI-generated from the topic \"{request.Topic}\".",
            IsFavorite = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        _db.FlashcardDecks.Add(deck);

        foreach (var (front, back) in generatedCards)
        {
            _db.Flashcards.Add(new Flashcard
            {
                Id = Guid.NewGuid(),
                DeckId = deck.Id,
                FrontText = front,
                BackText = back,
                EaseFactor = Sm2Scheduler.InitialEaseFactor,
                IntervalDays = 0,
                Repetitions = 0,
                NextReviewDateUtc = today,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(deck.Id);
    }
}
