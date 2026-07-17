using FluentValidation;

namespace StudyVerse.Application.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryValidator : AbstractValidator<GetNotificationsQuery>
{
    public GetNotificationsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Take).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
