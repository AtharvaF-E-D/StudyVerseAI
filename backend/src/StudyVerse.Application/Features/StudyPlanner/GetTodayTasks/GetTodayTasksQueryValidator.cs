using FluentValidation;

namespace StudyVerse.Application.Features.StudyPlanner.GetTodayTasks;

public sealed class GetTodayTasksQueryValidator : AbstractValidator<GetTodayTasksQuery>
{
    public GetTodayTasksQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
