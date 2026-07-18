using FluentValidation;

namespace StudyVerse.Application.Features.StudyPlanner.GetWeeklyTasks;

public sealed class GetWeeklyTasksQueryValidator : AbstractValidator<GetWeeklyTasksQuery>
{
    public GetWeeklyTasksQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
