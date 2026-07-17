using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Quiz;

namespace StudyVerse.Application.Features.Quiz.Common;

/// <summary>
/// Picks the questions for a new <see cref="QuizSession"/>: <see cref="QuizScoring.QuestionsPerSession"/>
/// questions from the requested category+difficulty, excluding any question the user has been
/// shown in their last <see cref="RecentSessionsToExclude"/> COMPLETED sessions (any category —
/// the anti-repetition rule is "don't repeat a question the user recently saw", not scoped to this
/// particular category/difficulty pairing), then randomizing order.
///
/// Fallback: if the fresh (never-recently-shown) pool for this category+difficulty is smaller
/// than <see cref="QuizScoring.QuestionsPerSession"/>, repeats are allowed to top up the session —
/// documented here per the phase spec, since a modestly-sized seeded question bank means most
/// category+difficulty cells won't have 10+ fresh questions on every attempt. If the category+
/// difficulty pool itself has fewer than 10 questions total, the session simply gets however many
/// exist (never actually 0 given the seed, but handled defensively by the caller).
/// </summary>
internal static class QuizQuestionSelectionService
{
    private const int RecentSessionsToExclude = 3;

    public static async Task<IReadOnlyList<QuizQuestion>> SelectQuestionsAsync(
        IAppDbContext db,
        Guid userId,
        string category,
        QuizDifficulty difficulty,
        CancellationToken cancellationToken)
    {
        var pool = await db.QuizQuestions
            .Where(q => q.Category == category && q.Difficulty == difficulty)
            .ToListAsync(cancellationToken);

        var recentSessionIds = await db.QuizSessions
            .Where(s => s.UserId == userId && s.Status == QuizSessionStatus.Completed)
            .OrderByDescending(s => s.StartedAtUtc)
            .Take(RecentSessionsToExclude)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var recentlyShownIds = recentSessionIds.Count == 0
            ? []
            : (await db.QuizSessionQuestions
                .Where(sq => recentSessionIds.Contains(sq.SessionId))
                .Select(sq => sq.QuestionId)
                .ToListAsync(cancellationToken))
                .ToHashSet();

        var fresh = pool.Where(q => !recentlyShownIds.Contains(q.Id)).ToList();

        var selectionPool = fresh.Count >= QuizScoring.QuestionsPerSession
            ? fresh
            // Fallback: not enough fresh questions to fill a full session — top up with the
            // recently-shown ones rather than starting a session short of QuestionsPerSession.
            : fresh.Concat(pool.Where(q => recentlyShownIds.Contains(q.Id))).ToList();

        return selectionPool
            .OrderBy(_ => Guid.NewGuid())
            .Take(QuizScoring.QuestionsPerSession)
            .ToList();
    }
}
