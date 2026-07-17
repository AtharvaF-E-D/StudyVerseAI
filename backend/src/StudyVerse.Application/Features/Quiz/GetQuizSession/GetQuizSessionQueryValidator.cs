using FluentValidation;

namespace StudyVerse.Application.Features.Quiz.GetQuizSession;

public sealed class GetQuizSessionQueryValidator : AbstractValidator<GetQuizSessionQuery>
{
    public GetQuizSessionQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
