using FluentAssertions;
using StudyVerse.Application.Features.Gamification.Common;
using StudyVerse.Application.Features.Gamification.GetBadges;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Gamification.GetBadges;

public sealed class GetBadgesQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetBadgesQueryHandler CreateHandler() => new(_db, new BadgeEvaluationService(_db, _dateTimeProvider));

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

    [Fact]
    public async Task Handle_UnknownUser_FailsWithNotFound()
    {
        var result = await CreateHandler().Handle(new GetBadgesQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ResultErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_WithNoActivity_ReturnsEveryCatalogBadgeUnearned()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetBadgesQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(BadgeCatalog.All.Count);
        result.Value.EarnedCount.Should().Be(0);
        result.Value.Badges.Should().HaveCount(BadgeCatalog.All.Count);
        result.Value.Badges.Should().OnlyContain(b => !b.IsEarned && b.EarnedAtUtc == null);
    }

    [Fact]
    public async Task Handle_WithRealFlashcardDeckActivity_EvaluatesAndReturnsBookwormAsEarned()
    {
        var userId = SeedUser();
        _db.FlashcardDecks.Add(new FlashcardDeck
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "Deck",
            CreatedAtUtc = _dateTimeProvider.UtcNow,
            UpdatedAtUtc = _dateTimeProvider.UtcNow,
        });
        _db.SaveChanges();

        var result = await CreateHandler().Handle(new GetBadgesQuery(userId), CancellationToken.None);

        result.Value.EarnedCount.Should().Be(1);
        var bookworm = result.Value.Badges.Single(b => b.Id == BadgeCatalog.BookwormId);
        bookworm.IsEarned.Should().BeTrue();
        bookworm.EarnedAtUtc.Should().NotBeNull();
    }
}
