using FluentValidation;
using StudyVerse.Application.Features.CodingPractice.Common;

namespace StudyVerse.Application.Features.CodingPractice.SubmitCode;

public sealed class SubmitCodeCommandValidator : AbstractValidator<SubmitCodeCommand>
{
    private const int MaxSourceCodeLength = 20_000;

    public SubmitCodeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProblemId).NotEmpty();

        RuleFor(x => x.LanguageId)
            .Must(SupportedLanguages.IsSupported)
            .WithMessage("Unsupported language id - see GET /languages for the supported set.");

        RuleFor(x => x.SourceCode)
            .NotEmpty()
            .MaximumLength(MaxSourceCodeLength);
    }
}
