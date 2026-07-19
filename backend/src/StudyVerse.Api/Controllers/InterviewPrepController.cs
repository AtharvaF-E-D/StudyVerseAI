using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.InterviewPrep.CompleteInterviewSession;
using StudyVerse.Application.Features.InterviewPrep.GetInterviewCategories;
using StudyVerse.Application.Features.InterviewPrep.GetInterviewSession;
using StudyVerse.Application.Features.InterviewPrep.GetInterviewSessions;
using StudyVerse.Application.Features.InterviewPrep.GetInterviewStats;
using StudyVerse.Application.Features.InterviewPrep.GetResumeAnalyses;
using StudyVerse.Application.Features.InterviewPrep.StartInterviewSession;
using StudyVerse.Application.Features.InterviewPrep.SubmitAnswer;
using StudyVerse.Application.Features.InterviewPrep.UploadResume;

namespace StudyVerse.Api.Controllers;

/// <summary>
/// Interview Preparation: HR/Technical/Behavioral text Q&amp;A practice sessions with real-time AI
/// grading (see <c>SubmitAnswerCommandHandler</c>) over a hand-seeded question bank (see
/// <c>InterviewQuestionSeedData</c>), plus AI resume analysis reusing the Phase 6 upload pipeline
/// (see <c>UploadResumeCommandHandler</c>) — same conventions as <see cref="NotesController"/>/
/// <see cref="CodingPracticeController"/>. Voice interviews are explicitly out of scope for this
/// pass (real STT/TTS is a separate, larger effort — same reasoning as the Tutor's deferred voice
/// input/output).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/interview")]
[Authorize]
public sealed class InterviewPrepController : ApiControllerBase
{
    // A little above UploadResumeCommandValidator's 10MB cap so oversized uploads still reach the
    // handler and get FluentValidation's friendly error message, rather than a raw 413 from Kestrel
    // - same reasoning as NotesController.MaxRequestBodyBytes.
    private const long MaxRequestBodyBytes = 12 * 1024 * 1024;

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetInterviewCategoriesQuery(), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartSession([FromBody] StartInterviewSessionRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new StartInterviewSessionCommand(userId, request.Type), cancellationToken);

        return FromResult(result, dto => Ok(dto));
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

        var result = await Mediator.Send(new GetInterviewSessionQuery(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("sessions/{id:guid}/answers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAnswer(Guid id, [FromBody] SubmitInterviewAnswerRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new SubmitAnswerCommand(userId, id, request.QuestionId, request.AnswerText),
            cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("sessions/{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteSession(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new CompleteInterviewSessionCommand(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSessions(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetInterviewSessionsQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("resume")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBodyBytes)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadResume(IFormFile? file, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new ApiErrorResponse("A file is required."));
        }

        await using var stream = file.OpenReadStream();
        var command = new UploadResumeCommand(userId, stream, file.FileName, file.ContentType, file.Length);

        var result = await Mediator.Send(command, cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("resume/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetResumeHistory(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetResumeAnalysesQuery(userId), cancellationToken);

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

        var result = await Mediator.Send(new GetInterviewStatsQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }
}
