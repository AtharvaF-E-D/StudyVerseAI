using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.AbandonQuizSession;

/// <summary>For a user quitting mid-quiz. No rewards are awarded — only <c>SubmitAnswerCommandHandler</c>
/// (on completion) ever credits XP/coins to <see cref="StudyVerse.Domain.Entities.UserProgress"/>.</summary>
public sealed class AbandonQuizSessionCommandHandler : IRequestHandler<AbandonQuizSessionCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AbandonQuizSessionCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(AbandonQuizSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.QuizSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure("Quiz session not found.", ResultErrorType.NotFound);
        }

        if (session.Status != QuizSessionStatus.InProgress)
        {
            return Result.Failure("This quiz session is already finished.", ResultErrorType.Conflict);
        }

        session.Status = QuizSessionStatus.Abandoned;
        session.EndedAtUtc = _dateTimeProvider.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
