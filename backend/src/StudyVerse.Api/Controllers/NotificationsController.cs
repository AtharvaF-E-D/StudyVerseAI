using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Application.Features.Notifications.GetNotifications;
using StudyVerse.Application.Features.Notifications.MarkNotificationRead;

namespace StudyVerse.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
public sealed class NotificationsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool onlyUnread,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        // Default here (rather than a C# default parameter) so it applies whether the query
        // string omits `take` entirely or passes it explicitly as empty.
        var effectiveTake = take <= 0 ? 20 : take;

        var result = await Mediator.Send(new GetNotificationsQuery(userId, onlyUnread, effectiveTake), cancellationToken);

        return FromResult(result, notifications => Ok(notifications));
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new MarkNotificationReadCommand(userId, id), cancellationToken);

        return FromResult(result, () => Ok(new { message = "Notification marked as read." }));
    }
}
