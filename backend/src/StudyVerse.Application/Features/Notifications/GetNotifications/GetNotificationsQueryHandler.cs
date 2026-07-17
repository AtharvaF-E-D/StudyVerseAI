using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    private readonly IAppDbContext _db;

    public GetNotificationsQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Notifications.Where(n => n.UserId == request.UserId);

        if (request.OnlyUnread)
        {
            query = query.Where(n => n.ReadAtUtc == null);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(request.Take)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Body, n.CreatedAtUtc, n.ReadAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<NotificationDto>>(notifications);
    }
}
