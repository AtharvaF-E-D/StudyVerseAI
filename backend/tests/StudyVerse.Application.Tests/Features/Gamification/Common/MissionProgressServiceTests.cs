using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Gamification.Common;

public sealed class MissionProgressServiceTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private MissionProgressService CreateService() => new(_db, _dateTimeProvider);

    private Guid SeedUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            DisplayName = "Student",
            AuthProvider = AuthProvider.Local,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user.Id;
    }

    private Guid SeedCodingProblem()
    {
        var problem = new CodingProblem
        {
            Id = Guid.NewGuid(),
            Title = "Problem " + Guid.NewGuid().ToString("N")[..6],
            Description = "Description.",
            Difficulty = CodingDifficulty.Easy,
            Category = "Arrays",
            IsInterviewQuestion = false,
            StarterCodeJson = "{}",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.CodingProblems.Add(problem);
        _db.SaveChanges();
        return problem.Id;
    }

    /// <summary>
    /// Advances <see cref="_dateTimeProvider"/> to a date (midday, to stay safely clear of week
    /// boundaries) within a week where <paramref name="metric"/> is one of that week's 3 active
    /// missions, and returns that template - so tests never have to hardcode which week a given
    /// metric rotates into.
    /// </summary>
    private MissionTemplate MoveToAWeekActiveFor(MissionMetric metric)
    {
        var startDate = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);
        for (var offset = 0; offset < 400; offset++)
        {
            var date = startDate.AddDays(offset);
            var template = WeeklyMissionSelector.GetThisWeeksTemplates(date).FirstOrDefault(t => t.Metric == metric);
            if (template is not null)
            {
                _dateTimeProvider.UtcNow = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddHours(12);
                return template;
            }
        }

        throw new InvalidOperationException($"No active week found for {metric} within 400 days - MissionCatalog may have changed.");
    }

    [Fact]
    public async Task RefreshThisWeeksMissionsAsync_BelowTarget_ReportsProgressButNotCompleted()
    {
        var userId = SeedUser();
        var template = MoveToAWeekActiveFor(MissionMetric.QuizSessionsCompleted);

        for (var i = 0; i < template.TargetCount - 1; i++)
        {
            _db.QuizSessions.Add(new QuizSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = "Science",
                Difficulty = QuizDifficulty.Easy,
                Status = QuizSessionStatus.Completed,
                StartedAtUtc = _dateTimeProvider.UtcNow,
                EndedAtUtc = _dateTimeProvider.UtcNow,
            });
        }
        _db.SaveChanges();

        var results = await CreateService().RefreshThisWeeksMissionsAsync(userId, CancellationToken.None);

        var quizMission = results.Single(r => r.Template.Id == template.Id);
        quizMission.CurrentCount.Should().Be(template.TargetCount - 1);
        quizMission.IsCompleted.Should().BeFalse();
        quizMission.CompletedAtUtc.Should().BeNull();

        (await _db.UserProgresses.CountAsync(p => p.UserId == userId)).Should().Be(0);
    }

    [Fact]
    public async Task RefreshThisWeeksMissionsAsync_ReachingTarget_CompletesAndAwardsXpAndCoinsExactlyOnce()
    {
        var userId = SeedUser();
        var template = MoveToAWeekActiveFor(MissionMetric.QuizSessionsCompleted);

        for (var i = 0; i < template.TargetCount; i++)
        {
            _db.QuizSessions.Add(new QuizSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Category = "Science",
                Difficulty = QuizDifficulty.Easy,
                Status = QuizSessionStatus.Completed,
                StartedAtUtc = _dateTimeProvider.UtcNow,
                EndedAtUtc = _dateTimeProvider.UtcNow,
            });
        }
        _db.SaveChanges();

        var service = CreateService();
        var firstRun = await service.RefreshThisWeeksMissionsAsync(userId, CancellationToken.None);
        var firstMissionResult = firstRun.Single(r => r.Template.Id == template.Id);
        firstMissionResult.IsCompleted.Should().BeTrue();
        firstMissionResult.CompletedAtUtc.Should().NotBeNull();

        var progressAfterFirst = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progressAfterFirst.Xp.Should().Be(template.XpReward);
        progressAfterFirst.Coins.Should().Be(template.CoinReward);

        // Refresh again without any new activity - already-completed, must NOT re-award.
        var secondRun = await service.RefreshThisWeeksMissionsAsync(userId, CancellationToken.None);
        var secondMissionResult = secondRun.Single(r => r.Template.Id == template.Id);
        secondMissionResult.IsCompleted.Should().BeTrue();
        secondMissionResult.CompletedAtUtc.Should().Be(firstMissionResult.CompletedAtUtc);

        var progressAfterSecond = await _db.UserProgresses.SingleAsync(p => p.UserId == userId);
        progressAfterSecond.Xp.Should().Be(template.XpReward);
        progressAfterSecond.Coins.Should().Be(template.CoinReward);
    }

    [Fact]
    public async Task RefreshThisWeeksMissionsAsync_CountsDistinctProblemsForCodingProblemsSolved()
    {
        var userId = SeedUser();
        var template = MoveToAWeekActiveFor(MissionMetric.CodingProblemsSolved);

        var problemId = SeedCodingProblem();
        // Two Accepted submissions for the SAME problem - counts once, not twice.
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(), UserId = userId, ProblemId = problemId, LanguageId = 109,
            SourceCode = "v1", Status = CodeSubmissionStatus.Accepted, TestsPassed = 1, TotalTests = 1,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(), UserId = userId, ProblemId = problemId, LanguageId = 109,
            SourceCode = "v2", Status = CodeSubmissionStatus.Accepted, TestsPassed = 1, TotalTests = 1,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        var resultsAfterOneProblem = await CreateService().RefreshThisWeeksMissionsAsync(userId, CancellationToken.None);
        resultsAfterOneProblem.Single(r => r.Template.Id == template.Id).CurrentCount.Should().Be(1);

        // A second, distinct problem now brings the distinct count to 2 (this template's target).
        var secondProblemId = SeedCodingProblem();
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(), UserId = userId, ProblemId = secondProblemId, LanguageId = 109,
            SourceCode = "v1", Status = CodeSubmissionStatus.Accepted, TestsPassed = 1, TotalTests = 1,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        var resultsAfterTwoProblems = await CreateService().RefreshThisWeeksMissionsAsync(userId, CancellationToken.None);
        var finalResult = resultsAfterTwoProblems.Single(r => r.Template.Id == template.Id);
        finalResult.CurrentCount.Should().Be(2);
        finalResult.IsCompleted.Should().Be(template.TargetCount <= 2);
    }

    [Fact]
    public async Task RefreshThisWeeksMissionsAsync_ActivityFromLastWeek_DoesNotCountTowardThisWeek()
    {
        var userId = SeedUser();
        var template = MoveToAWeekActiveFor(MissionMetric.NewsArticlesBookmarked);

        var article = new NewsArticle
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString("N"),
            Title = "Old article",
            Content = "Content",
            Url = "https://example.com/old",
            Category = "general",
            SourceName = "Example News",
            PublishedAtUtc = _dateTimeProvider.UtcNow.AddDays(-10),
            FetchedAtUtc = _dateTimeProvider.UtcNow.AddDays(-10),
        };
        _db.NewsArticles.Add(article);
        // Bookmarked 10 days ago - before this week started.
        _db.NewsBookmarks.Add(new NewsBookmark
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArticleId = article.Id,
            CreatedAtUtc = _dateTimeProvider.UtcNow.AddDays(-10),
        });
        _db.SaveChanges();

        var results = await CreateService().RefreshThisWeeksMissionsAsync(userId, CancellationToken.None);

        results.Single(r => r.Template.Id == template.Id).CurrentCount.Should().Be(0);
    }
}
