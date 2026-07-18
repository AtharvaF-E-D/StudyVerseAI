using FluentValidation;

namespace StudyVerse.Application.Features.StudyPlanner.CompleteTask;

public sealed class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
{
    public CompleteTaskCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
