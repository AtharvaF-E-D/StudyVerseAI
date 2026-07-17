using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.UseFiftyFifty;

public sealed class UseFiftyFiftyCommandHandler : IRequestHandler<UseFiftyFiftyCommand, Result<FiftyFiftyResultDto>>
{
    private readonly IAppDbContext _db;

    public UseFiftyFiftyCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<FiftyFiftyResultDto>> Handle(UseFiftyFiftyCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.QuizSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<FiftyFiftyResultDto>("Quiz session not found.", ResultErrorType.NotFound);
        }

        if (session.Status != QuizSessionStatus.InProgress)
        {
            return Result.Failure<FiftyFiftyResultDto>(
                "This quiz session is no longer in progress.",
                ResultErrorType.Conflict);
        }

        if (session.UsedFiftyFifty)
        {
            return Result.Failure<FiftyFiftyResultDto>(
                "Fifty-fifty has already been used this session.",
                ResultErrorType.Conflict);
        }

        var currentQuestionId = await _db.QuizSessionQuestions
            .Where(sq => sq.SessionId == session.Id && sq.OrderIndex == session.CurrentQuestionIndex)
            .Select(sq => sq.QuestionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentQuestionId == Guid.Empty)
        {
            return Result.Failure<FiftyFiftyResultDto>(
                "This quiz session has no current question.",
                ResultErrorType.Conflict);
        }

        var correctOptionIndex = await _db.QuizQuestions
            .Where(q => q.Id == currentQuestionId)
            .Select(q => q.CorrectOptionIndex)
            .FirstAsync(cancellationToken);

        // Hide 2 of the 3 incorrect options at random, leaving the correct option plus exactly one
        // wrong option visible — a genuine 50/50.
        var hiddenOptionIndexes = Enumerable.Range(0, 4)
            .Where(i => i != correctOptionIndex)
            .OrderBy(_ => Guid.NewGuid())
            .Take(2)
            .ToList();

        session.UsedFiftyFifty = true;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new FiftyFiftyResultDto(hiddenOptionIndexes));
    }
}
