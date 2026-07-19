using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Application.Features.Gamification.ClaimDailyReward;
using StudyVerse.Application.Features.Gamification.GetBadges;
using StudyVerse.Application.Features.Gamification.GetDailyRewardStatus;
using StudyVerse.Application.Features.Gamification.GetMissions;
using StudyVerse.Application.Features.Gamification.GetSpinStatus;
using StudyVerse.Application.Features.Gamification.GetSummary;
using StudyVerse.Application.Features.Gamification.Spin;

namespace StudyVerse.Api.Controllers;

/// <summary>
/// Gamification (Phase 13): badges detected from real activity across every other feature, weekly
/// missions, daily login rewards, a daily spin wheel, and a seasonal event bonus. XP/coins/levels/
/// streaks/leaderboard already exist from Phase 3 - see <see cref="DashboardController"/> and
/// <see cref="LeaderboardController"/> for those.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/gamification")]
[Authorize]
public sealed class GamificationController : ApiControllerBase
{
    [HttpGet("badges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBadges(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetBadgesQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("missions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMissions(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetMissionsQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("daily-reward/claim")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ClaimDailyReward(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new ClaimDailyRewardCommand(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("daily-reward/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDailyRewardStatus(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetDailyRewardStatusQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("spin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Spin(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new SpinCommand(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("spin/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSpinStatus(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetSpinStatusQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetGamificationSummaryQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }
}
