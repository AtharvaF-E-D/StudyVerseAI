using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notifications.MarkNotificationRead;

/// <summary>
/// Marking an already-read notification as read again is still a success (idempotent), not an
/// error — the client doesn't need to track whether it already sent this request.
/// </summary>
public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MarkNotificationReadCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(
            n => n.Id == request.NotificationId && n.UserId == request.UserId,
            cancellationToken);

        if (notification is null)
        {
            return Result.Failure("Notification not found.", ResultErrorType.NotFound);
        }

        if (notification.ReadAtUtc is null)
        {
            notification.ReadAtUtc = _dateTimeProvider.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
