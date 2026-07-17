using FluentAssertions;
using StudyVerse.Application.Features.Tutor.GetConversations;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Application.Tests.Features.Tutor.GetConversations;

public sealed class GetConversationsQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetConversationsQueryHandler CreateHandler() => new(_db);

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

    private Conversation SeedConversation(Guid userId, string title, DateTime updatedAtUtc)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            CreatedAtUtc = updatedAtUtc,
            UpdatedAtUtc = updatedAtUtc,
        };
        _db.Conversations.Add(conversation);
        _db.SaveChanges();
        return conversation;
    }

    [Fact]
    public async Task Handle_WithNoSearch_ReturnsAllOfThisUsersConversationsOrderedByMostRecentlyUpdated()
    {
        var userId = SeedUser();
        SeedConversation(userId, "Algebra basics", _dateTimeProvider.UtcNow.AddHours(-2));
        SeedConversation(userId, "Photosynthesis", _dateTimeProvider.UtcNow.AddHours(-1));
        SeedConversation(Guid.NewGuid(), "Someone else's chat", _dateTimeProvider.UtcNow);

        var result = await CreateHandler().Handle(new GetConversationsQuery(userId, null, 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(c => c.Title).Should().ContainInOrder("Photosynthesis", "Algebra basics");
    }

    [Fact]
    public async Task Handle_WithASearchTerm_FiltersToTitlesContainingItCaseInsensitively()
    {
        var userId = SeedUser();
        SeedConversation(userId, "Algebra basics", _dateTimeProvider.UtcNow.AddHours(-2));
        SeedConversation(userId, "Photosynthesis explained", _dateTimeProvider.UtcNow.AddHours(-1));
        SeedConversation(userId, "More ALGEBRA questions", _dateTimeProvider.UtcNow);

        var result = await CreateHandler().Handle(new GetConversationsQuery(userId, "algebra", 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(c => c.Title).Should().BeEquivalentTo("Algebra basics", "More ALGEBRA questions");
    }

    [Fact]
    public async Task Handle_WithASearchTermMatchingNothing_ReturnsAnEmptyList()
    {
        var userId = SeedUser();
        SeedConversation(userId, "Algebra basics", _dateTimeProvider.UtcNow);

        var result = await CreateHandler().Handle(new GetConversationsQuery(userId, "chemistry", 20), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
