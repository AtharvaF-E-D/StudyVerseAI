using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Flashcards.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.SpacedRepetition;

namespace StudyVerse.Application.Features.Flashcards.ReviewCard;

public sealed class ReviewCardCommandHandler : IRequestHandler<ReviewCardCommand, Result<ReviewCardResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReviewCardCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ReviewCardResultDto>> Handle(ReviewCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _db.Flashcards.FirstOrDefaultAsync(
            c => c.Id == request.CardId && c.Deck!.UserId == request.UserId,
            cancellationToken);

        if (card is null)
        {
            return Result.Failure<ReviewCardResultDto>("Card not found.", ResultErrorType.NotFound);
        }

        var now = _dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var currentState = new Sm2CardState(card.EaseFactor, card.IntervalDays, card.Repetitions);
        var newState = Sm2Scheduler.Schedule(currentState, (int)request.Quality, today);

        card.EaseFactor = newState.EaseFactor;
        card.IntervalDays = newState.IntervalDays;
        card.Repetitions = newState.Repetitions;
        card.NextReviewDateUtc = newState.NextReviewDateUtc;
        card.LastReviewedAtUtc = now;

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new ReviewCardResultDto(
            card.Id, card.EaseFactor, card.IntervalDays, card.Repetitions, card.NextReviewDateUtc, now);

        return Result.Success(dto);
    }
}
