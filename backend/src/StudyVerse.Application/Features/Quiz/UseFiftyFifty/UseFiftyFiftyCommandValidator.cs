using FluentValidation;

namespace StudyVerse.Application.Features.Quiz.UseFiftyFifty;

public sealed class UseFiftyFiftyCommandValidator : AbstractValidator<UseFiftyFiftyCommand>
{
    public UseFiftyFiftyCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
