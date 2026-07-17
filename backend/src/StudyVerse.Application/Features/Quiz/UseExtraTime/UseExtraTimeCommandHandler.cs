using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Features.Quiz.UseExtraTime;

public sealed class UseExtraTimeCommandHandler : IRequestHandler<UseExtraTimeCommand, Result<UseExtraTimeResultDto>>
{
    private readonly IAppDbContext _db;

    public UseExtraTimeCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<UseExtraTimeResultDto>> Handle(UseExtraTimeCommand request, CancellationToken cancellationToken)
    {
        var session = await _db.QuizSessions.FirstOrDefaultAsync(
            s => s.Id == request.SessionId && s.UserId == request.UserId,
            cancellationToken);

        if (session is null)
        {
            return Result.Failure<UseExtraTimeResultDto>("Quiz session not found.", ResultErrorType.NotFound);
        }

        if (session.Status != QuizSessionStatus.InProgress)
        {
            return Result.Failure<UseExtraTimeResultDto>(
                "This quiz session is no longer in progress.",
                ResultErrorType.Conflict);
        }

        if (session.UsedExtraTime)
        {
            return Result.Failure<UseExtraTimeResultDto>(
                "Extra time has already been used this session.",
                ResultErrorType.Conflict);
        }

        session.UsedExtraTime = true;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new UseExtraTimeResultDto(true));
    }
}
