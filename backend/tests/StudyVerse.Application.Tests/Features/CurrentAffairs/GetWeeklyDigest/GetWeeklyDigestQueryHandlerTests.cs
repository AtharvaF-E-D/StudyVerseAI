using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.GetWeeklyDigest;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.CurrentAffairs.GetWeeklyDigest;

public sealed class GetWeeklyDigestQueryHandlerTests
{
    // 2026-07-15 is a Wednesday, so MostRecentMonday resolves to 2026-07-13.
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc) };
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly IAiChatProvider _aiChatProvider = Substitute.For<IAiChatProvider>();

    private GetWeeklyDigestQueryHandler CreateHandler() => new(_db, _aiChatProvider, _dateTimeProvider);

    private void SeedArticleThisWeek(string category, string title)
    {
        _db.NewsArticles.Add(new NewsArticle
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString("N"),
            Title = title,
            Description = "A description.",
            Content = "Content",
            Url = $"https://example.com/{title}",
            Category = category,
            SourceName = "Example News",
            PublishedAtUtc = _dateTimeProvider.UtcNow,
            FetchedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_NoArticlesCachedThisWeek_FailsWithNotFoundWithoutCallingTheAi()
    {
        var result = await CreateHandler().Handle(new GetWeeklyDigestQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
        await _aiChatProvider.DidNotReceiveWithAnyArgs().GetCompletionAsync(default!, default, default);
        (await _db.WeeklyDigests.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_ArticlesCachedThisWeek_GeneratesAndPersistsADigest()
    {
        SeedArticleThisWeek("technology", "AI Breakthrough");
        SeedArticleThisWeek("business", "Markets Rally");
        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(new AiChatResult("This week saw major developments across technology and business.", 200, 300));

        var result = await CreateHandler().Handle(new GetWeeklyDigestQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SummaryText.Should().Contain("major developments");
        result.Value.WeekStartDateUtc.Should().Be(new DateOnly(2026, 7, 13));
        (await _db.WeeklyDigests.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_ArticlesExistButOutsideThisWeek_FailsWithNotFound()
    {
        SeedArticleThisWeek("technology", "Last Week's Story");
        var lastWeeksArticle = await _db.NewsArticles.SingleAsync();
        lastWeeksArticle.PublishedAtUtc = _dateTimeProvider.UtcNow.AddDays(-10);
        await _db.SaveChangesAsync();

        var result = await CreateHandler().Handle(new GetWeeklyDigestQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ADigestAlreadyExistsForThisWeek_ReturnsItWithoutCallingTheAiAgain()
    {
        SeedArticleThisWeek("technology", "AI Breakthrough");
        _aiChatProvider
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>())
            .Returns(new AiChatResult("Digest text.", 200, 300));
        var handler = CreateHandler();

        var firstResult = await handler.Handle(new GetWeeklyDigestQuery(), CancellationToken.None);
        var secondResult = await handler.Handle(new GetWeeklyDigestQuery(), CancellationToken.None);

        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.SummaryText.Should().Be(firstResult.Value.SummaryText);
        await _aiChatProvider.Received(1)
            .GetCompletionAsync(Arg.Any<IReadOnlyList<AiChatMessage>>(), Arg.Any<CancellationToken>(), Arg.Any<bool>());
        (await _db.WeeklyDigests.CountAsync()).Should().Be(1);
    }
}
