using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Notes.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Features.Flashcards.GenerateDeckFromNote;

public sealed class GenerateDeckFromNoteCommandHandler : IRequestHandler<GenerateDeckFromNoteCommand, Result<Guid>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GenerateDeckFromNoteCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid>> Handle(GenerateDeckFromNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _db.Notes
            .Include(n => n.Content)
            .FirstOrDefaultAsync(n => n.Id == request.NoteId && n.UserId == request.UserId, cancellationToken);

        if (note is null)
        {
            return Result.Failure<Guid>("Note not found.", ResultErrorType.NotFound);
        }

        if (note.Status != NoteStatus.Ready || note.Content is null)
        {
            return Result.Failure<Guid>("This note hasn't finished generating its AI content yet.");
        }

        var flashcards = NoteAiResponseMapper.FromEntity(note.Content).Flashcards;
        if (flashcards.Count == 0)
        {
            return Result.Failure<Guid>("This note has no generated flashcards to copy into a deck.");
        }

        var now = _dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var deck = new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = note.Title,
            Description = $"Generated from your note \"{note.Title}\".",
            IsFavorite = false,
            SourceNoteId = note.Id,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        _db.FlashcardDecks.Add(deck);

        foreach (var card in flashcards)
        {
            _db.Flashcards.Add(new Flashcard
            {
                Id = Guid.NewGuid(),
                DeckId = deck.Id,
                FrontText = card.Question,
                BackText = card.Answer,
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
