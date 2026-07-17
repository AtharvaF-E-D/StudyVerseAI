using FluentValidation;

namespace StudyVerse.Application.Features.Dashboard.GetDashboard;

public sealed class GetDashboardQueryValidator : AbstractValidator<GetDashboardQuery>
{
    public GetDashboardQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
