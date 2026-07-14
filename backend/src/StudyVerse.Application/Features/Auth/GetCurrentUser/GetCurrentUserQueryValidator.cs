using FluentValidation;

namespace StudyVerse.Application.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
{
    public GetCurrentUserQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
