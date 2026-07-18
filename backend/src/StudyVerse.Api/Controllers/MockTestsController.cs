using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.MockTests.GetMockTestAttempt;
using StudyVerse.Application.Features.MockTests.GetMockTestAttempts;
using StudyVerse.Application.Features.MockTests.GetMockTestReview;
using StudyVerse.Application.Features.MockTests.GetMockTestTemplates;
using StudyVerse.Application.Features.MockTests.StartMockTestAttempt;
using StudyVerse.Application.Features.MockTests.SubmitMockTestAttempt;

namespace StudyVerse.Api.Controllers;

/// <summary>Mock Tests: full-length, timed exam simulations built over the shared Rapid Fire Quiz question bank (see <c>MockTestCatalog</c>).</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mocktests")]
[Authorize]
public sealed class MockTestsController : ApiControllerBase
{
    [HttpGet("templates")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMockTestTemplatesQuery(), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("attempts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartAttempt([FromBody] StartMockTestAttemptRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new StartMockTestAttemptCommand(userId, request.TemplateId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("attempts/{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAttempt(Guid id, [FromBody] SubmitMockTestAttemptRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var answers = request.Answers
            .Select(a => new MockTestAnswerInput(a.QuestionId, a.SelectedOptionIndex))
            .ToList();

        var result = await Mediator.Send(new SubmitMockTestAttemptCommand(userId, id, answers), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("attempts/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttempt(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetMockTestAttemptQuery(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("attempts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAttempts(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetMockTestAttemptsQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("attempts/{id:guid}/review")]
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

        var result = await Mediator.Send(new GetMockTestReviewQuery(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }
}
