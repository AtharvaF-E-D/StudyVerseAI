using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Gamification.Common;

/// <summary>One mission template's refreshed-this-request progress, ready for either <c>GetMissionsQueryHandler</c> or the gamification summary to shape into their own DTOs.</summary>
public sealed record MissionProgressResult(
    MissionTemplate Template,
    int CurrentCount,
    bool IsCompleted,
    DateTime? CompletedAtUtc);

/// <summary>
/// Recomputes this week's active missions' progress from the real underlying tables and persists it
/// - shared by <c>GetMissionsQueryHandler</c> and <c>GetGamificationSummaryQueryHandler</c>, the same
/// "one shared builder, two call sites" shape as <c>LeaderboardBuilder</c>. Awards XP/coins to
/// <see cref="UserProgress"/> exactly once, the moment a mission's <c>IsCompleted</c> flips from
/// false to true (never on subsequent calls, since that flag is persisted and never reset within
/// the same week) - deliberately does not react to other features' handlers directly; progress is
/// recomputed fresh on every read instead ("check-on-read", the same reasoning
/// <c>BadgeEvaluationService</c> uses).
/// </summary>
public sealed class MissionProgressService
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MissionProgressService(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyList<MissionProgressResult>> RefreshThisWeeksMissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var weekStart = WeeklyMissionSelector.GetWeekStartDateUtc(today);
        var activeTemplates = WeeklyMissionSelector.GetThisWeeksTemplates(today);
        var activeTemplateIds = activeTemplates.Select(t => t.Id).ToList();

        var existingProgress = await _db.UserMissionProgresses
            .Where(p => p.UserId == userId && p.WeekStartDateUtc == weekStart && activeTemplateIds.Contains(p.MissionTemplateId))
            .ToListAsync(cancellationToken);
        var existingProgressByTemplateId = existingProgress.ToDictionary(p => p.MissionTemplateId);

        var results = new List<MissionProgressResult>(activeTemplates.Count);
        var anyChanges = false;

        foreach (var template in activeTemplates)
        {
            var currentCount = await ComputeCurrentCountAsync(userId, template.Metric, weekStart, cancellationToken);
            var isCompleted = currentCount >= template.TargetCount;

            if (!existingProgressByTemplateId.TryGetValue(template.Id, out var progress))
            {
                progress = new UserMissionProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    MissionTemplateId = template.Id,
                    WeekStartDateUtc = weekStart,
                    CurrentCount = 0,
                    IsCompleted = false,
                };
                _db.UserMissionProgresses.Add(progress);
            }

            var justCompleted = isCompleted && !progress.IsCompleted;

            if (progress.CurrentCount != currentCount || progress.IsCompleted != isCompleted)
            {
                anyChanges = true;
            }

            progress.CurrentCount = currentCount;
            progress.IsCompleted = isCompleted;

            if (justCompleted)
            {
                progress.CompletedAtUtc = now;

                var userProgress = await _db.UserProgresses.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
                if (userProgress is null)
                {
                    userProgress = new UserProgress { UserId = userId };
                    _db.UserProgresses.Add(userProgress);
                }

                userProgress.Xp += template.XpReward;
                userProgress.Coins += template.CoinReward;
                anyChanges = true;
            }

            results.Add(new MissionProgressResult(template, currentCount, isCompleted, progress.CompletedAtUtc));
        }

        if (anyChanges)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return results;
    }

    private async Task<int> ComputeCurrentCountAsync(
        Guid userId,
        MissionMetric metric,
        DateOnly weekStart,
        CancellationToken cancellationToken)
    {
        var weekStartUtc = weekStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var weekEndUtcExclusive = weekStart.AddDays(7).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        switch (metric)
        {
            case MissionMetric.QuizSessionsCompleted:
                return await _db.QuizSessions.CountAsync(
                    s => s.UserId == userId
                         && s.Status == QuizSessionStatus.Completed
                         && s.EndedAtUtc != null
                         && s.EndedAtUtc >= weekStartUtc
                         && s.EndedAtUtc < weekEndUtcExclusive,
                    cancellationToken);

            case MissionMetric.CodingProblemsSolved:
                return await _db.CodeSubmissions
                    .Where(s => s.UserId == userId
                                && s.Status == CodeSubmissionStatus.Accepted
                                && s.SubmittedAtUtc >= weekStartUtc
                                && s.SubmittedAtUtc < weekEndUtcExclusive)
                    .Select(s => s.ProblemId)
                    .Distinct()
                    .CountAsync(cancellationToken);

            case MissionMetric.StudyDaysActive:
                return await ComputeStudyDaysActiveAsync(userId, weekStartUtc, weekEndUtcExclusive, cancellationToken);

            case MissionMetric.FlashcardsReviewed:
                return await _db.Flashcards.CountAsync(
                    f => f.Deck!.UserId == userId
                         && f.LastReviewedAtUtc != null
                         && f.LastReviewedAtUtc >= weekStartUtc
                         && f.LastReviewedAtUtc < weekEndUtcExclusive,
                    cancellationToken);

            case MissionMetric.NewsArticlesBookmarked:
                return await _db.NewsBookmarks.CountAsync(
                    b => b.UserId == userId && b.CreatedAtUtc >= weekStartUtc && b.CreatedAtUtc < weekEndUtcExclusive,
                    cancellationToken);

            default:
                throw new ArgumentOutOfRangeException(nameof(metric), metric, "Unknown mission metric.");
        }
    }

    /// <summary>
    /// Distinct calendar days within the mission week with real activity in any of three
    /// representative tables (quiz sessions started, code submitted, flashcards reviewed) - not
    /// every activity table in the app, but a genuine cross-section, documented on
    /// <see cref="MissionMetric.StudyDaysActive"/>. Dates are converted to <see cref="DateOnly"/>
    /// client-side after materializing the (already week-filtered) timestamps, since not every EF
    /// provider translates <c>DateOnly.FromDateTime</c> in a query.
    /// </summary>
    private async Task<int> ComputeStudyDaysActiveAsync(
        Guid userId,
        DateTime weekStartUtc,
        DateTime weekEndUtcExclusive,
        CancellationToken cancellationToken)
    {
        var quizTimestamps = await _db.QuizSessions
            .Where(s => s.UserId == userId && s.StartedAtUtc >= weekStartUtc && s.StartedAtUtc < weekEndUtcExclusive)
            .Select(s => s.StartedAtUtc)
            .ToListAsync(cancellationToken);

        var codeTimestamps = await _db.CodeSubmissions
            .Where(s => s.UserId == userId && s.SubmittedAtUtc >= weekStartUtc && s.SubmittedAtUtc < weekEndUtcExclusive)
            .Select(s => s.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        var flashcardTimestamps = await _db.Flashcards
            .Where(f => f.Deck!.UserId == userId
                        && f.LastReviewedAtUtc != null
                        && f.LastReviewedAtUtc >= weekStartUtc
                        && f.LastReviewedAtUtc < weekEndUtcExclusive)
            .Select(f => f.LastReviewedAtUtc!.Value)
            .ToListAsync(cancellationToken);

        return quizTimestamps
            .Concat(codeTimestamps)
            .Concat(flashcardTimestamps)
            .Select(DateOnly.FromDateTime)
            .Distinct()
            .Count();
    }
}
