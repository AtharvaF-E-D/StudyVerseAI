using FluentAssertions;
using NSubstitute;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.Gamification.GetSpinStatus;
using StudyVerse.Application.Features.Gamification.Spin;
using StudyVerse.Application.Tests.TestSupport;
using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;
using StudyVerse.Domain.Gamification;

namespace StudyVerse.Application.Tests.Features.Gamification.GetSpinStatus;

public sealed class GetSpinStatusQueryHandlerTests
{
    private readonly TestAppDbContext _db = TestDbContextFactory.Create();
    private readonly TestDateTimeProvider _dateTimeProvider = new();

    private GetSpinStatusQueryHandler CreateHandler() => new(_db, _dateTimeProvider);

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
    public async Task Handle_BeforeSpinning_ReportsNotSpun()
    {
        var userId = SeedUser();

        var result = await CreateHandler().Handle(new GetSpinStatusQuery(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.SpunToday.Should().BeFalse();
        result.Value.TodaysPrizeLabel.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AfterSpinning_ReportsSpunWithThePrizeLabel()
    {
        var userId = SeedUser();
        var randomProvider = Substitute.For<IRandomProvider>();
        randomProvider.Next(0, SpinPrizeCatalog.TotalWeight).Returns(0);
        await new SpinCommandHandler(_db, _dateTimeProvider, randomProvider)
            .Handle(new SpinCommand(userId), CancellationToken.None);

        var result = await CreateHandler().Handle(new GetSpinStatusQuery(userId), CancellationToken.None);

        result.Value.SpunToday.Should().BeTrue();
        result.Value.TodaysPrizeLabel.Should().Be(SpinPrizeCatalog.All[0].Label);
    }
}
