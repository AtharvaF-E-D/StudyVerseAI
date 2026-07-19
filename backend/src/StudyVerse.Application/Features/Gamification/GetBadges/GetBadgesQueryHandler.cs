using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Gamification.GetBadges;

/// <summary>Evaluate-then-return: runs <see cref="BadgeEvaluationService"/> for the user first, then reports every catalog badge's earned state.</summary>
public sealed class GetBadgesQueryHandler : IRequestHandler<GetBadgesQuery, Result<GetBadgesResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly BadgeEvaluationService _badgeEvaluationService;

    public GetBadgesQueryHandler(IAppDbContext db, BadgeEvaluationService badgeEvaluationService)
    {
        _db = db;
        _badgeEvaluationService = badgeEvaluationService;
    }

    public async Task<Result<GetBadgesResultDto>> Handle(GetBadgesQuery request, CancellationToken cancellationToken)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<GetBadgesResultDto>("User not found.", ResultErrorType.NotFound);
        }

        await _badgeEvaluationService.EvaluateAsync(request.UserId, cancellationToken);

        var earnedAtByBadgeId = await _db.UserBadges
            .Where(b => b.UserId == request.UserId)
            .ToDictionaryAsync(b => b.BadgeId, b => b.EarnedAtUtc, cancellationToken);

        var badges = BadgeCatalog.All
            .Select(b =>
            {
                var isEarned = earnedAtByBadgeId.TryGetValue(b.Id, out var earnedAt);
                return new BadgeDto(b.Id, b.Title, b.Description, b.Category, isEarned, isEarned ? earnedAt : null);
            })
            .ToList();

        var result = new GetBadgesResultDto(earnedAtByBadgeId.Count, BadgeCatalog.All.Count, badges);

        return Result.Success(result);
    }
}
