using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : IRequest<Result>;
