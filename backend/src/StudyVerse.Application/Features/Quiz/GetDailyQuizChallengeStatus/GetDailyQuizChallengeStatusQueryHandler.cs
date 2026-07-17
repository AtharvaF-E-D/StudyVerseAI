using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Features.Quiz.GetDailyQuizChallengeStatus;

public sealed class GetDailyQuizChallengeStatusQueryHandler
    : IRequestHandler<GetDailyQuizChallengeStatusQuery, Result<DailyQuizChallengeStatusDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetDailyQuizChallengeStatusQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<DailyQuizChallengeStatusDto>> Handle(
        GetDailyQuizChallengeStatusQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var (category, difficulty) = DailyQuizSelector.GetTodaysChallenge(today);

        var alreadyAttemptedToday = await _db.QuizSessions.AnyAsync(
            s => s.UserId == request.UserId && s.DailyChallengeDateUtc == today,
            cancellationToken);

        return Result.Success(new DailyQuizChallengeStatusDto(category, difficulty, alreadyAttemptedToday));
    }
}
