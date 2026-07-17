using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notifications.GetNotifications;

public sealed record GetNotificationsQuery(Guid UserId, bool OnlyUnread, int Take)
    : IRequest<Result<IReadOnlyList<NotificationDto>>>;
