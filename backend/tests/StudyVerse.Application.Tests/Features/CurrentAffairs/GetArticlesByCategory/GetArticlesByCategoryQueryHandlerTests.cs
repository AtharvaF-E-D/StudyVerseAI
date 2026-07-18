using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.GetArticlesByCategory;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.CurrentAffairs;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.CurrentAffairs.GetArticlesByCategory;

public sealed class GetArticlesByCategoryQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 12, 0, 0, DateTimeKind.Utc) };
    private readonly IGNewsProvider _gNewsProvider = Substitute.For<IGNewsProvider>();

    private GetArticlesByCategoryQueryHandler CreateHandler() => new(_db, _gNewsProvider, _dateTimeProvider);

    private static GNewsArticleDto MakeFetchedArticle(string externalId, string title, DateTime publishedAtUtc) =>
        new(externalId, title, "A description.", "Full content.", $"https://example.com/{externalId}", null, "Example News", publishedAtUtc);

    private void SeedCachedArticle(string externalId, string category, DateTime fetchedAtUtc)
    {
        _db.NewsArticles.Add(new NewsArticle
        {
            Id = Guid.NewGuid(),
            ExternalId = externalId,
            Title = "Cached Story",
            Description = "Cached description.",
            Content = "Cached content.",
            Url = $"https://example.com/{externalId}",
            Category = category,
            SourceName = "Example News",
            PublishedAtUtc = fetchedAtUtc,
            FetchedAtUtc = fetchedAtUtc,
        });
        _db.SaveChanges();
    }

    [Fact]
    public async Task Handle_NoCachedArticlesForCategory_FetchesFromGNewsAndPersists()
    {
        _gNewsProvider.GetTopHeadlinesAsync(NewsCategories.Technology, Arg.Any<CancellationToken>())
            .Returns([MakeFetchedArticle("ext-1", "Story One", _dateTimeProvider.UtcNow)]);

        var result = await CreateHandler().Handle(new GetArticlesByCategoryQuery(NewsCategories.Technology, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(a => a.Title == "Story One");
        await _gNewsProvider.Received(1).GetTopHeadlinesAsync(NewsCategories.Technology, Arg.Any<CancellationToken>());
        (await _db.NewsArticles.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_CacheIsFresh_DoesNotCallGNewsAndReturnsCachedArticles()
    {
        SeedCachedArticle("ext-cached", NewsCategories.Technology, _dateTimeProvider.UtcNow.AddHours(-1));

        var result = await CreateHandler().Handle(new GetArticlesByCategoryQuery(NewsCategories.Technology, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(a => a.Title == "Cached Story");
        await _gNewsProvider.DidNotReceiveWithAnyArgs().GetTopHeadlinesAsync(default!, default);
    }

    [Fact]
    public async Task Handle_NoArticlesFetchedYet_TreatsTheCacheAsStaleAndCallsGNews()
    {
        // No cached rows at all for this category - "none exist" branch of the staleness check,
        // distinct from "fetched more than 6 hours ago".
        _gNewsProvider.GetTopHeadlinesAsync(NewsCategories.Business, Arg.Any<CancellationToken>())
            .Returns([MakeFetchedArticle("ext-biz", "Markets Rally", _dateTimeProvider.UtcNow)]);

        var result = await CreateHandler().Handle(new GetArticlesByCategoryQuery(NewsCategories.Business, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _gNewsProvider.Received(1).GetTopHeadlinesAsync(NewsCategories.Business, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CacheIsStale_CallsGNewsAgainAndAddsTheNewArticle()
    {
        SeedCachedArticle("ext-old", NewsCategories.Technology, _dateTimeProvider.UtcNow.AddHours(-7));
        _gNewsProvider.GetTopHeadlinesAsync(NewsCategories.Technology, Arg.Any<CancellationToken>())
            .Returns([MakeFetchedArticle("ext-new", "Fresh Story", _dateTimeProvider.UtcNow)]);

        var result = await CreateHandler().Handle(new GetArticlesByCategoryQuery(NewsCategories.Technology, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _gNewsProvider.Received(1).GetTopHeadlinesAsync(NewsCategories.Technology, Arg.Any<CancellationToken>());
        (await _db.NewsArticles.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Handle_FetchedArticleExternalIdAlreadyExists_DoesNotInsertADuplicate()
    {
        SeedCachedArticle("ext-dup", NewsCategories.Technology, _dateTimeProvider.UtcNow.AddHours(-7));
        _gNewsProvider.GetTopHeadlinesAsync(NewsCategories.Technology, Arg.Any<CancellationToken>())
            .Returns([MakeFetchedArticle("ext-dup", "Same Story, Refetched", _dateTimeProvider.UtcNow)]);

        var result = await CreateHandler().Handle(new GetArticlesByCategoryQuery(NewsCategories.Technology, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _db.NewsArticles.CountAsync()).Should().Be(1);
        // The pre-existing row (and its original title) is kept, not overwritten by the refetch.
        (await _db.NewsArticles.SingleAsync()).Title.Should().Be("Cached Story");
    }

    [Fact]
    public async Task Handle_GNewsReturnsNothing_StillReturnsWhateverWasAlreadyCached()
    {
        SeedCachedArticle("ext-old", NewsCategories.Technology, _dateTimeProvider.UtcNow.AddHours(-7));
        _gNewsProvider.GetTopHeadlinesAsync(NewsCategories.Technology, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await CreateHandler().Handle(new GetArticlesByCategoryQuery(NewsCategories.Technology, 10), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(a => a.Title == "Cached Story");
    }
}
