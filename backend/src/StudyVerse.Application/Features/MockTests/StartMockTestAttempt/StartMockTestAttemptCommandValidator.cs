using FluentValidation;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Features.MockTests.StartMockTestAttempt;

public sealed class StartMockTestAttemptCommandValidator : AbstractValidator<StartMockTestAttemptCommand>
{
    public StartMockTestAttemptCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.TemplateId)
            .NotEmpty()
            .Must(id => MockTestCatalog.All.Any(t => t.Id == id))
            .WithMessage("TemplateId must match one of the known mock test templates.");
    }
}
