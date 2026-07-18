using FluentAssertions;
using StudyVerse.Application.Features.CurrentAffairs.GetBookmarks;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.CurrentAffairs.GetBookmarks;

public sealed class GetBookmarksQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetBookmarksQueryHandler CreateHandler() => new(_db);

    private NewsArticle SeedArticle(string title)
    {
        var article = new NewsArticle
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString("N"),
            Title = title,
            Content = "Content",
            Url = $"https://example.com/{title}",
            Category = "technology",
            SourceName = "Example News",
            PublishedAtUtc = _dateTimeProvider.UtcNow,
            FetchedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.NewsArticles.Add(article);
        _db.SaveChanges();
        return article;
    }

    [Fact]
    public async Task Handle_ReturnsOnlyThisUsersBookmarks_MostRecentlyBookmarkedFirst()
    {
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var articleOne = SeedArticle("Story One");
        var articleTwo = SeedArticle("Story Two");
        var othersArticle = SeedArticle("Someone Else's Story");

        _db.NewsBookmarks.Add(new NewsBookmark
        {
            Id = Guid.NewGuid(), UserId = userId, ArticleId = articleOne.Id, CreatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.NewsBookmarks.Add(new NewsBookmark
        {
            Id = Guid.NewGuid(), UserId = userId, ArticleId = articleTwo.Id, CreatedAtUtc = _dateTimeProvider.UtcNow.AddMinutes(5),
        });
        _db.NewsBookmarks.Add(new NewsBookmark
        {
            Id = Guid.NewGuid(), UserId = otherUserId, ArticleId = othersArticle.Id, CreatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetBookmarksQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Title.Should().Be("Story Two");
        result.Value[1].Title.Should().Be("Story One");
    }
}
