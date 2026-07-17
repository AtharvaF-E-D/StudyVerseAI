using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Dashboard.CompleteChallenge;

public sealed class CompleteChallengeCommandHandler
    : IRequestHandler<CompleteChallengeCommand, Result<CompleteChallengeResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CompleteChallengeCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<CompleteChallengeResultDto>> Handle(
        CompleteChallengeCommand request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var template = DailyChallengeSelector.GetTodaysTemplates(today)
            .FirstOrDefault(t => t.Id == request.ChallengeTemplateId);

        if (template is null)
        {
            return Result.Failure<CompleteChallengeResultDto>(
                "That challenge is not one of today's challenges.",
                ResultErrorType.Validation);
        }

        // Pre-check for a friendly error message; the unique index on
        // (UserId, ChallengeTemplateId, CompletedDateUtc) is the actual source of truth guarding
        // against a concurrent duplicate completion.
        var alreadyCompleted = await _db.ChallengeCompletions.AnyAsync(
            c => c.UserId == request.UserId
                 && c.ChallengeTemplateId == request.ChallengeTemplateId
                 && c.CompletedDateUtc == today,
            cancellationToken);

        if (alreadyCompleted)
        {
            return Result.Failure<CompleteChallengeResultDto>(
                "You already completed this challenge today.",
                ResultErrorType.Conflict);
        }

        var progress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        if (progress is null)
        {
            progress = new UserProgress { UserId = request.UserId };
            _db.UserProgresses.Add(progress);
        }

        progress.Xp += template.XpReward;
        progress.Coins += template.CoinReward;

        _db.ChallengeCompletions.Add(new ChallengeCompletion
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ChallengeTemplateId = request.ChallengeTemplateId,
            CompletedDateUtc = today,
            CompletedAtUtc = _dateTimeProvider.UtcNow,
            XpAwarded = template.XpReward,
            CoinsAwarded = template.CoinReward,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new CompleteChallengeResultDto(
            template.XpReward,
            template.CoinReward,
            progress.Xp,
            progress.Coins,
            LevelCalculator.GetLevel(progress.Xp)));
    }
}
