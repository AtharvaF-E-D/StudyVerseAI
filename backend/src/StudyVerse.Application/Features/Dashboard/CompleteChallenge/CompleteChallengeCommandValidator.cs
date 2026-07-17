using FluentValidation;

namespace StudyVerse.Application.Features.Dashboard.CompleteChallenge;

public sealed class CompleteChallengeCommandValidator : AbstractValidator<CompleteChallengeCommand>
{
    public CompleteChallengeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ChallengeTemplateId).NotEmpty();
    }
}
