using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.Quiz.AbandonQuizSession;
using StudyVerse.Application.Features.Quiz.GetDailyQuizChallengeStatus;
using StudyVerse.Application.Features.Quiz.GetQuizCategories;
using StudyVerse.Application.Features.Quiz.GetQuizReview;
using StudyVerse.Application.Features.Quiz.GetQuizSession;
using StudyVerse.Application.Features.Quiz.GetQuizStats;
using StudyVerse.Application.Features.Quiz.StartQuizSession;
using StudyVerse.Application.Features.Quiz.SubmitAnswer;
using StudyVerse.Application.Features.Quiz.UseExtraTime;
using StudyVerse.Application.Features.Quiz.UseFiftyFifty;

namespace StudyVerse.Api.Controllers;

/// <summary>Rapid Fire Quiz: timed multiple-choice sessions from a static, seeded question bank (see <c>QuizQuestionSeedData</c>).</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/quiz")]
[Authorize]
public sealed class QuizController : ApiControllerBase
{
    [HttpPost("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartSession([FromBody] StartQuizSessionRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new StartQuizSessionCommand(userId, request.Category, request.Difficulty, request.IsDailyChallenge),
            cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("sessions/{id:guid}/answers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAnswer(Guid id, [FromBody] SubmitAnswerRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new SubmitAnswerCommand(userId, id, request.QuestionId, request.SelectedOptionIndex, request.TimeTakenMs),
            cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("sessions/{id:guid}/power-ups/fifty-fifty")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UseFiftyFifty(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new UseFiftyFiftyCommand(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("sessions/{id:guid}/power-ups/extra-time")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UseExtraTime(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new UseExtraTimeCommand(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("sessions/{id:guid}/abandon")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AbandonSession(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new AbandonQuizSessionCommand(userId, id), cancellationToken);

        return FromResult(result, () => NoContent());
    }

    [HttpGet("sessions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetQuizSessionQuery(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("sessions/{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReview(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetQuizReviewQuery(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetQuizCategoriesQuery(), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetQuizStatsQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("daily-challenge/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDailyChallengeStatus(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetDailyQuizChallengeStatusQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }
}
