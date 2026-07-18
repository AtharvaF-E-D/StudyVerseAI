using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Application.Features.CurrentAffairs.SearchArticles;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.CurrentAffairs.SearchArticles;

public sealed class SearchArticlesQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new() { UtcNow = new DateTime(2026, 7, 18, 12, 0, 0, DateTimeKind.Utc) };
    private readonly IGNewsProvider _gNewsProvider = Substitute.For<IGNewsProvider>();

    private SearchArticlesQueryHandler CreateHandler() => new(_db, _gNewsProvider, _dateTimeProvider);

    [Fact]
    public async Task Handle_AlwaysCallsSearch_EvenWithNoCachedArticles()
    {
        _gNewsProvider.SearchAsync("elections", Arg.Any<CancellationToken>())
            .Returns([new GNewsArticleDto("ext-1", "Election Results", "desc", "content", "https://example.com/1", null, "Example News", _dateTimeProvider.UtcNow)]);

        var result = await CreateHandler().Handle(new SearchArticlesQuery("elections"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(a => a.Title == "Election Results");
        await _gNewsProvider.Received(1).SearchAsync("elections", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ANewSearchResult_IsUpsertedUnderTheSearchPseudoCategory()
    {
        _gNewsProvider.SearchAsync("space", Arg.Any<CancellationToken>())
            .Returns([new GNewsArticleDto("ext-space", "Space News", "desc", "content", "https://example.com/space", null, "Example News", _dateTimeProvider.UtcNow)]);

        await CreateHandler().Handle(new SearchArticlesQuery("space"), CancellationToken.None);

        var persisted = await _db.NewsArticles.SingleAsync(a => a.ExternalId == "ext-space");
        persisted.Category.Should().Be(NewsArticleUpsertService.SearchPseudoCategory);
    }

    [Fact]
    public async Task Handle_SearchResultMatchesAnArticleAlreadyCachedFromACategoryFetch_KeepsItsRealCategory()
    {
        _db.NewsArticles.Add(new NewsArticle
        {
            Id = Guid.NewGuid(),
            ExternalId = "ext-known",
            Title = "Known Story",
            Content = "Content",
            Url = "https://example.com/known",
            Category = "technology",
            SourceName = "Example News",
            PublishedAtUtc = _dateTimeProvider.UtcNow,
            FetchedAtUtc = _dateTimeProvider.UtcNow,
        });
        await _db.SaveChangesAsync();

        _gNewsProvider.SearchAsync("known", Arg.Any<CancellationToken>())
            .Returns([new GNewsArticleDto("ext-known", "Known Story", "desc", "content", "https://example.com/known", null, "Example News", _dateTimeProvider.UtcNow)]);

        var result = await CreateHandler().Handle(new SearchArticlesQuery("known"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _db.NewsArticles.CountAsync()).Should().Be(1);
        (await _db.NewsArticles.SingleAsync()).Category.Should().Be("technology");
    }
}
