using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.GetArticleQuiz;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.CurrentAffairs.GetArticleQuiz;

public sealed class GetArticleQuizQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();
    private readonly IAiChatProvider _aiChatProvider = Substitute.For<IAiChatProvider>();

    private GetArticleQuizQueryHandler CreateHandler() => new(_db, _aiChatProvider, _dateTimeProvider);

    private Guid SeedArticle()
    {
        var article = new NewsArticle
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString("N"),
            Title = "A Big Story",
            Content = "Something significant happened today in the world of science.",
            Url = "https://example.com/story",
            Category = "science",
            SourceName = "Example News",
            PublishedAtUtc = _dateTimeProvider.UtcNow,
            FetchedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.NewsArticles.Add(article);
        _db.SaveChanges();
        return article.Id;
    }

    private void StubAiQuiz()
    {
        var payload = new
        {
            questions = new[]
            {
                new
                {
                    questionText = "What happened?",
                    options = new[] { "A", "B", "C", "D" },
                    correctOptionIndex = 1,
                    explanation = "Because B.",
                },
            },
        };
        var json = JsonSerializer.Serialize(payload);

        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(new AiChatResult(json, 100, 50));
    }

    [Fact]
    public async Task Handle_FirstRequestForAnArticle_CallsTheAiAndPersistsTheQuiz()
    {
        var articleId = SeedArticle();
        StubAiQuiz();

        var result = await CreateHandler().Handle(new GetArticleQuizQuery(articleId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Questions.Should().ContainSingle(q => q.QuestionText == "What happened?" && q.CorrectOptionIndex == 1);
        await _aiChatProvider.Received(1)
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>());
        (await _db.NewsArticleQuizzes.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_SecondRequestForTheSameArticle_ReturnsTheCachedQuizWithoutCallingTheAiAgain()
    {
        var articleId = SeedArticle();
        StubAiQuiz();
        var handler = CreateHandler();

        var firstResult = await handler.Handle(new GetArticleQuizQuery(articleId), CancellationToken.None);
        var secondResult = await handler.Handle(new GetArticleQuizQuery(articleId), CancellationToken.None);

        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.Questions.Should().BeEquivalentTo(firstResult.Value.Questions);
        secondResult.Value.GeneratedAtUtc.Should().Be(firstResult.Value.GeneratedAtUtc);
        // The critical assertion: still exactly one AI call total, across both requests.
        await _aiChatProvider.Received(1)
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>());
        (await _db.NewsArticleQuizzes.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_ArticleDoesNotExist_FailsWithNotFoundWithoutCallingTheAi()
    {
        var result = await CreateHandler().Handle(new GetArticleQuizQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        await _aiChatProvider.DidNotReceiveWithAnyArgs().GetCompletionAsync(default!, default, default);
    }

    [Fact]
    public async Task Handle_AiReturnsNoUsableQuestions_FailsWithoutPersistingAQuiz()
    {
        var articleId = SeedArticle();
        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(new AiChatResult("{ \"questions\": [] }", 10, 5));

        var result = await CreateHandler().Handle(new GetArticleQuizQuery(articleId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        (await _db.NewsArticleQuizzes.CountAsync()).Should().Be(0);
    }
}
