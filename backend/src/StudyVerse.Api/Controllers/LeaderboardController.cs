using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Application.Features.Leaderboard.GetLeaderboard;

namespace StudyVerse.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/leaderboard")]
[Authorize]
public sealed class LeaderboardController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLeaderboard([FromQuery] int take, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var effectiveTake = take <= 0 ? 20 : take;

        var result = await Mediator.Send(new GetLeaderboardQuery(userId, effectiveTake), cancellationToken);

        return FromResult(result, entries => Ok(entries));
    }
}
