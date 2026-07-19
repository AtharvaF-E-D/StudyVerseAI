using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Gamification.GetMissions;

public sealed class GetMissionsQueryHandler : IRequestHandler<GetMissionsQuery, Result<GetMissionsResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly MissionProgressService _missionProgressService;

    public GetMissionsQueryHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider, MissionProgressService missionProgressService)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
        _missionProgressService = missionProgressService;
    }

    public async Task<Result<GetMissionsResultDto>> Handle(GetMissionsQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<GetMissionsResultDto>("User not found.", ResultErrorType.NotFound);
        }

        var progressResults = await _missionProgressService.RefreshThisWeeksMissionsAsync(request.UserId, cancellationToken);

        var missions = progressResults
            .Select(r => new MissionDto(
                r.Template.Id,
                r.Template.Title,
                r.Template.Description,
                r.Template.TargetCount,
                Math.Min(r.CurrentCount, r.Template.TargetCount),
                r.IsCompleted,
                r.CompletedAtUtc,
                r.Template.XpReward,
                r.Template.CoinReward))
            .ToList();

        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        var weekStart = WeeklyMissionSelector.GetWeekStartDateUtc(today);

        var result = new GetMissionsResultDto(
            weekStart,
            missions.Count(m => m.IsCompleted),
            missions.Count,
            missions);

        return Result.Success(result);
    }
}
