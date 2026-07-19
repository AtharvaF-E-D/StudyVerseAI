using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Features.Gamification.Common;

/// <summary>
/// Given a user, checks every <see cref="BadgeCatalog"/> badge the user hasn't already earned
/// against their real activity across every relevant table, and inserts a <see cref="UserBadge"/>
/// row for any newly-met one. Called lazily by <c>GetBadgesQueryHandler</c> (and the gamification
/// summary) on every read — "evaluate, then return" — rather than being hooked into every other
/// feature's handlers, which would be far more invasive for a single time-boxed pass. Not behind an
/// interface, same reasoning as <c>MissedTaskRecoveryService</c>: it depends only on
/// <see cref="IAppDbContext"/>/<see cref="IDateTimeProvider"/>, so there's nothing a test double
/// would need to substitute beyond those.
/// </summary>
public sealed class BadgeEvaluationService
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BadgeEvaluationService(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>Evaluates and persists any newly-earned badges for <paramref name="userId"/>. Idempotent — safe to call on every read.</summary>
    public async Task EvaluateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var earnedBadgeIds = await _db.UserBadges
            .Where(b => b.UserId == userId)
            .Select(b => b.BadgeId)
            .ToListAsync(cancellationToken);
        var earnedSet = earnedBadgeIds.ToHashSet();

        // Nothing left to check — skip every query below.
        if (earnedSet.Count == BadgeCatalog.All.Count)
        {
            return;
        }

        var completedQuizSessions = await _db.QuizSessions
            .CountAsync(s => s.UserId == userId && s.Status == QuizSessionStatus.Completed, cancellationToken);

        var hasFlashcardDeck = await _db.FlashcardDecks.AnyAsync(d => d.UserId == userId, cancellationToken);

        var acceptedDistinctProblemCount = await _db.CodeSubmissions
            .Where(s => s.UserId == userId && s.Status == CodeSubmissionStatus.Accepted)
            .Select(s => s.ProblemId)
            .Distinct()
            .CountAsync(cancellationToken);

        var hasMockTestSubmitted = await _db.MockTestAttempts
            .AnyAsync(a => a.UserId == userId && a.Status == MockTestAttemptStatus.Submitted, cancellationToken);

        var hasConversationWithMessage = await _db.Conversations
            .AnyAsync(c => c.UserId == userId && c.Messages.Any(), cancellationToken);

        var hasStudyPlan = await _db.StudyPlans.AnyAsync(p => p.UserId == userId, cancellationToken);

        var hasNewsBookmark = await _db.NewsBookmarks.AnyAsync(b => b.UserId == userId, cancellationToken);

        var hasCompletedInterview = await _db.InterviewSessions
            .AnyAsync(s => s.UserId == userId && s.Status == InterviewSessionStatus.Completed, cancellationToken);

        var longestStreakDays = await _db.UserProgresses
            .Where(p => p.UserId == userId)
            .Select(p => (int?)p.LongestStreakDays)
            .FirstOrDefaultAsync(cancellationToken) ?? 0;

        var activeFeatureAreaCount = new[]
        {
            completedQuizSessions >= 1,
            hasFlashcardDeck,
            acceptedDistinctProblemCount >= 1,
            hasMockTestSubmitted,
            hasConversationWithMessage,
            hasStudyPlan,
            hasNewsBookmark,
            hasCompletedInterview,
        }.Count(met => met);

        var newlyEarnedBadgeIds = new List<Guid>();

        void CheckBadge(Guid badgeId, bool isMet)
        {
            if (isMet && !earnedSet.Contains(badgeId))
            {
                newlyEarnedBadgeIds.Add(badgeId);
            }
        }

        CheckBadge(BadgeCatalog.FirstStepsId, completedQuizSessions >= 1);
        CheckBadge(BadgeCatalog.BookwormId, hasFlashcardDeck);
        CheckBadge(BadgeCatalog.CodeWarriorId, acceptedDistinctProblemCount >= 1);
        CheckBadge(BadgeCatalog.ScholarId, hasMockTestSubmitted);
        CheckBadge(BadgeCatalog.ChatterboxId, hasConversationWithMessage);
        CheckBadge(BadgeCatalog.PlannerId, hasStudyPlan);
        CheckBadge(BadgeCatalog.NewsHoundId, hasNewsBookmark);
        CheckBadge(BadgeCatalog.InterviewReadyId, hasCompletedInterview);
        CheckBadge(BadgeCatalog.WeekWarriorId, longestStreakDays >= 7);
        CheckBadge(BadgeCatalog.QuizMasterId, completedQuizSessions >= 10);
        CheckBadge(BadgeCatalog.CodeMasterId, acceptedDistinctProblemCount >= 10);
        CheckBadge(BadgeCatalog.WellRoundedId, activeFeatureAreaCount >= 6);

        if (newlyEarnedBadgeIds.Count == 0)
        {
            return;
        }

        var now = _dateTimeProvider.UtcNow;
        foreach (var badgeId in newlyEarnedBadgeIds)
        {
            _db.UserBadges.Add(new UserBadge
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BadgeId = badgeId,
                EarnedAtUtc = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
