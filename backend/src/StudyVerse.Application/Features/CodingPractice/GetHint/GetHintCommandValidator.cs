using FluentValidation;

namespace StudyVerse.Application.Features.CodingPractice.GetHint;

public sealed class GetHintCommandValidator : AbstractValidator<GetHintCommand>
{
    public GetHintCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProblemId).NotEmpty();
        // CurrentCode may legitimately be empty (the student hasn't written anything yet) - see
        // CodingHintPromptBuilder, which handles that case explicitly. Not validated here.
    }
}
