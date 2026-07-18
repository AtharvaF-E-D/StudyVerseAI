using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Features.CurrentAffairs.ToggleBookmark;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Tests.Features.CurrentAffairs.ToggleBookmark;

public sealed class ToggleBookmarkCommandHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private ToggleBookmarkCommandHandler CreateHandler() => new(_db, _dateTimeProvider);

    private Guid SeedArticle()
    {
        var article = new NewsArticle
        {
            Id = Guid.NewGuid(),
            ExternalId = Guid.NewGuid().ToString("N"),
            Title = "Some Story",
            Content = "Content",
            Url = "https://example.com/story",
            Category = "technology",
            SourceName = "Example News",
            PublishedAtUtc = _dateTimeProvider.UtcNow,
            FetchedAtUtc = _dateTimeProvider.UtcNow,
        };
        _db.NewsArticles.Add(article);
        _db.SaveChanges();
        return article.Id;
    }

    [Fact]
    public async Task Handle_FirstCall_BookmarksTheArticle()
    {
        var articleId = SeedArticle();
        var userId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new ToggleBookmarkCommand(userId, articleId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsBookmarked.Should().BeTrue();
        (await _db.NewsBookmarks.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_SecondCallForTheSameUserAndArticle_RemovesTheBookmark()
    {
        var articleId = SeedArticle();
        var userId = Guid.NewGuid();
        var handler = CreateHandler();
        await handler.Handle(new ToggleBookmarkCommand(userId, articleId), CancellationToken.None);

        var result = await handler.Handle(new ToggleBookmarkCommand(userId, articleId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsBookmarked.Should().BeFalse();
        (await _db.NewsBookmarks.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_ArticleDoesNotExist_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(
            new ToggleBookmarkCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_TwoDifferentUsersBookmarkTheSameArticle_TogglingOneDoesNotAffectTheOther()
    {
        var articleId = SeedArticle();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var handler = CreateHandler();

        await handler.Handle(new ToggleBookmarkCommand(userA, articleId), CancellationToken.None);
        await handler.Handle(new ToggleBookmarkCommand(userB, articleId), CancellationToken.None);
        (await _db.NewsBookmarks.CountAsync()).Should().Be(2);

        var toggleOffResult = await handler.Handle(new ToggleBookmarkCommand(userA, articleId), CancellationToken.None);

        toggleOffResult.Value.IsBookmarked.Should().BeFalse();
        (await _db.NewsBookmarks.CountAsync()).Should().Be(1);
        (await _db.NewsBookmarks.SingleAsync()).UserId.Should().Be(userB);
    }
}
