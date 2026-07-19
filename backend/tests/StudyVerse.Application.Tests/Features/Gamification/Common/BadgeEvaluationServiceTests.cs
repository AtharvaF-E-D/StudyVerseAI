using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Gamification.Common;

public sealed class BadgeEvaluationServiceTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private BadgeEvaluationService CreateService() => new(_db, _dateTimeProvider);

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
            Title = "Two Sum",
            Description = "Find two numbers that add up to a target.",
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

    private async Task<HashSet<Guid>> GetEarnedBadgeIdsAsync(Guid userId) =>
        (await _db.UserBadges.Where(b => b.UserId == userId).Select(b => b.BadgeId).ToListAsync()).ToHashSet();

    [Fact]
    public async Task EvaluateAsync_WithNoActivityAtAll_EarnsNoBadges()
    {
        var userId = SeedUser();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await _db.UserBadges.CountAsync(b => b.UserId == userId)).Should().Be(0);
    }

    [Fact]
    public async Task EvaluateAsync_WithOneCompletedQuizSession_EarnsFirstStepsButNotQuizMaster()
    {
        var userId = SeedUser();
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
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        var earned = await GetEarnedBadgeIdsAsync(userId);
        earned.Should().Contain(BadgeCatalog.FirstStepsId);
        earned.Should().NotContain(BadgeCatalog.QuizMasterId);
    }

    [Fact]
    public async Task EvaluateAsync_With10CompletedQuizSessions_EarnsQuizMasterToo()
    {
        var userId = SeedUser();
        for (var i = 0; i < 10; i++)
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

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        var earned = await GetEarnedBadgeIdsAsync(userId);
        earned.Should().Contain(BadgeCatalog.FirstStepsId);
        earned.Should().Contain(BadgeCatalog.QuizMasterId);
    }

    [Fact]
    public async Task EvaluateAsync_WithAFlashcardDeck_EarnsBookworm()
    {
        var userId = SeedUser();
        _db.FlashcardDecks.Add(new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Biology 101",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().Contain(BadgeCatalog.BookwormId);
    }

    [Fact]
    public async Task EvaluateAsync_WithOneAcceptedSubmission_EarnsCodeWarriorButNotCodeMaster()
    {
        var userId = SeedUser();
        var problemId = SeedCodingProblem();
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProblemId = problemId,
            LanguageId = 109,
            SourceCode = "print('hi')",
            Status = CodeSubmissionStatus.Accepted,
            TestsPassed = 1,
            TotalTests = 1,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        var earned = await GetEarnedBadgeIdsAsync(userId);
        earned.Should().Contain(BadgeCatalog.CodeWarriorId);
        earned.Should().NotContain(BadgeCatalog.CodeMasterId);
    }

    [Fact]
    public async Task EvaluateAsync_With10DistinctAcceptedProblems_EarnsCodeMaster()
    {
        var userId = SeedUser();
        for (var i = 0; i < 10; i++)
        {
            var problemId = SeedCodingProblem();
            _db.CodeSubmissions.Add(new CodeSubmission
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProblemId = problemId,
                LanguageId = 109,
                SourceCode = "print('hi')",
                Status = CodeSubmissionStatus.Accepted,
                TestsPassed = 1,
                TotalTests = 1,
                SubmittedAtUtc = _dateTimeProvider.UtcNow,
            });
        }
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().Contain(BadgeCatalog.CodeMasterId);
    }

    [Fact]
    public async Task EvaluateAsync_WithOnlyAWrongAnswerSubmission_DoesNotEarnCodeWarrior()
    {
        var userId = SeedUser();
        var problemId = SeedCodingProblem();
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProblemId = problemId,
            LanguageId = 109,
            SourceCode = "print('wrong')",
            Status = CodeSubmissionStatus.WrongAnswer,
            TestsPassed = 0,
            TotalTests = 1,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().NotContain(BadgeCatalog.CodeWarriorId);
    }

    [Fact]
    public async Task EvaluateAsync_WithASubmittedMockTestAttempt_EarnsScholar()
    {
        var userId = SeedUser();
        _db.MockTestAttempts.Add(new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = Guid.NewGuid(),
            Status = MockTestAttemptStatus.Submitted,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
            CorrectCount = 5,
            TotalQuestions = 10,
            Score = 50,
        });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().Contain(BadgeCatalog.ScholarId);
    }

    [Fact]
    public async Task EvaluateAsync_WithAConversationThatHasAMessage_EarnsChatterbox()
    {
        var userId = SeedUser();
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "New conversation",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Conversations.Add(conversation);
        _db.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = "Explain photosynthesis",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().Contain(BadgeCatalog.ChatterboxId);
    }

    [Fact]
    public async Task EvaluateAsync_WithAnEmptyConversationAndNoMessages_DoesNotEarnChatterbox()
    {
        var userId = SeedUser();
        _db.Conversations.Add(new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "New conversation",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().NotContain(BadgeCatalog.ChatterboxId);
    }

    [Fact]
    public async Task EvaluateAsync_WithLongestStreakOfAtLeast7_EarnsWeekWarrior()
    {
        var userId = SeedUser();
        _db.UserProgresses.Add(new UserProgress { UserId = userId, LongestStreakDays = 7, CurrentStreakDays = 3 });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().Contain(BadgeCatalog.WeekWarriorId);
    }

    [Fact]
    public async Task EvaluateAsync_WithLongestStreakBelow7_DoesNotEarnWeekWarrior()
    {
        var userId = SeedUser();
        _db.UserProgresses.Add(new UserProgress { UserId = userId, LongestStreakDays = 6, CurrentStreakDays = 6 });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().NotContain(BadgeCatalog.WeekWarriorId);
    }

    [Fact]
    public async Task EvaluateAsync_CalledTwice_DoesNotDuplicateOrChangeTheEarnedAtTimestamp()
    {
        var userId = SeedUser();
        _db.FlashcardDecks.Add(new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Biology 101",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        var service = CreateService();
        await service.EvaluateAsync(userId, CancellationToken.None);
        var firstEarnedAt = await _db.UserBadges
            .Where(b => b.UserId == userId && b.BadgeId == BadgeCatalog.BookwormId)
            .Select(b => b.EarnedAtUtc)
            .SingleAsync();

        // Time moves forward and the same (already-earned) condition is still true - re-evaluating
        // must not insert a second row or bump the original EarnedAtUtc.
        _dateTimeProvider.UtcNow = _dateTimeProvider.UtcNow.AddDays(1);
        await service.EvaluateAsync(userId, CancellationToken.None);

        (await _db.UserBadges.CountAsync(b => b.UserId == userId && b.BadgeId == BadgeCatalog.BookwormId)).Should().Be(1);
        var secondEarnedAt = await _db.UserBadges
            .Where(b => b.UserId == userId && b.BadgeId == BadgeCatalog.BookwormId)
            .Select(b => b.EarnedAtUtc)
            .SingleAsync();
        secondEarnedAt.Should().Be(firstEarnedAt);
    }

    [Fact]
    public async Task EvaluateAsync_WithRealActivityInSixOrMoreFeatureAreas_EarnsWellRounded()
    {
        var userId = SeedUser();

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
        _db.FlashcardDecks.Add(new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Deck",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });
        var problemId = SeedCodingProblem();
        _db.CodeSubmissions.Add(new CodeSubmission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProblemId = problemId,
            LanguageId = 109,
            SourceCode = "print(1)",
            Status = CodeSubmissionStatus.Accepted,
            TestsPassed = 1,
            TotalTests = 1,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.MockTestAttempts.Add(new MockTestAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = Guid.NewGuid(),
            Status = MockTestAttemptStatus.Submitted,
            StartedAtUtc = _dateTimeProvider.UtcNow,
            SubmittedAtUtc = _dateTimeProvider.UtcNow,
            CorrectCount = 1,
            TotalQuestions = 1,
            Score = 100,
        });
        _db.StudyPlans.Add(new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExamDate = DateOnly.FromDateTime(_dateTimeProvider.UtcNow).AddMonths(1),
            HoursPerDayMinutes = 60,
            Status = StudyPlanStatus.Active,
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        });
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "New conversation",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.Conversations.Add(conversation);
        _db.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = "Hi",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        // That's 6 areas: Quiz, Flashcards, Coding, Mock Tests, Study Planner, AI Tutor.
        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().Contain(BadgeCatalog.WellRoundedId);
    }

    [Fact]
    public async Task EvaluateAsync_WithActivityInOnlyThreeFeatureAreas_DoesNotEarnWellRounded()
    {
        var userId = SeedUser();

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
        _db.FlashcardDecks.Add(new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Deck",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.NewsBookmarks.Add(new NewsBookmark
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ArticleId = SeedNewsArticle(),
            CreatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        await CreateService().EvaluateAsync(userId, CancellationToken.None);

        (await GetEarnedBadgeIdsAsync(userId)).Should().NotContain(BadgeCatalog.WellRoundedId);
    }

    private Guid SeedNewsArticle()
    {
        var article = new NewsArticle
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString("N"),
            Title = "Sample Article",
            Content = "Content",
            Url = "https://example.com/article",
            Category = "general",
            SourceName = "Example News",
            PublishedAtUtc = _dateTimeProvider.UtcNow,
            FetchedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.NewsArticles.Add(article);
        _db.SaveChanges();
        return article.Id;
    }
}
