using FluentAssertions;
using StudyVerse.Application.Features.StudyPlanner.CreateStudyPlan;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.CreateStudyPlan;

public sealed class CreateStudyPlanCommandValidatorTests
{
    private readonly CreateStudyPlanCommandValidator _validator = new();

    private static DateOnly FutureExamDate => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);

    [Fact]
    public void Validate_NullSubjectsList_FailsCleanlyRatherThanThrowing()
    {
        var command = new CreateStudyPlanCommand(Guid.NewGuid(), FutureExamDate, null!, [], 60);

        var act = () => _validator.Validate(command);

        act.Should().NotThrow();
        act().IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NullWeakTopicsList_FailsCleanlyRatherThanThrowing()
    {
        var command = new CreateStudyPlanCommand(Guid.NewGuid(), FutureExamDate, ["Math"], null!, 60);

        var act = () => _validator.Validate(command);

        act.Should().NotThrow();
        act().IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_EmptySubjectsList_HasValidationError()
    {
        var command = new CreateStudyPlanCommand(Guid.NewGuid(), FutureExamDate, [], [], 60);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStudyPlanCommand.Subjects));
    }

    [Fact]
    public void Validate_EmptyWeakTopicsList_IsValid()
    {
        var command = new CreateStudyPlanCommand(Guid.NewGuid(), FutureExamDate, ["Math"], [], 60);

        var result = _validator.Validate(command);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateStudyPlanCommand.WeakTopics));
    }

    [Fact]
    public void Validate_ValidCommand_HasNoValidationErrors()
    {
        var command = new CreateStudyPlanCommand(Guid.NewGuid(), FutureExamDate, ["Math", "Physics"], ["Physics"], 120);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(14)]
    [InlineData(1000)]
    public void Validate_HoursPerDayMinutesOutOfBounds_HasValidationError(int minutes)
    {
        var command = new CreateStudyPlanCommand(Guid.NewGuid(), FutureExamDate, ["Math"], [], minutes);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStudyPlanCommand.HoursPerDayMinutes));
    }

    [Fact]
    public void Validate_MissingUserId_Fails()
    {
        var command = new CreateStudyPlanCommand(Guid.Empty, FutureExamDate, ["Math"], [], 60);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateStudyPlanCommand.UserId));
    }
}
