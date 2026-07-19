using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.CodingPractice.Common;
using StudyVerse.Application.Features.CodingPractice.GetCodingStats;
using StudyVerse.Application.Features.CodingPractice.GetDailyCodingChallenge;
using StudyVerse.Application.Features.CodingPractice.GetHint;
using StudyVerse.Application.Features.CodingPractice.GetProblem;
using StudyVerse.Application.Features.CodingPractice.GetProblems;
using StudyVerse.Application.Features.CodingPractice.GetSubmissions;
using StudyVerse.Application.Features.CodingPractice.GetSupportedLanguages;
using StudyVerse.Application.Features.CodingPractice.SubmitCode;

namespace StudyVerse.Api.Controllers;

/// <summary>
/// Coding Practice: a real, hand-seeded problem bank (see <c>CodingProblemSeedData</c>) graded by
/// the real Judge0 CE API (see <c>Judge0Provider</c>), a daily rotating challenge, AI hints (reusing
/// the tutor's <c>IAiChatProvider</c>), and per-user progress/stats - same conventions as
/// <see cref="CurrentAffairsController"/>.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/coding")]
[Authorize]
public sealed class CodingPracticeController : ApiControllerBase
{
    [HttpGet("problems")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProblems(
        [FromQuery] string? difficulty,
        [FromQuery] string? category,
        [FromQuery] bool? interviewOnly,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetProblemsQuery(userId, difficulty, category, interviewOnly), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("problems/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProblem(Guid id, [FromQuery] int? languageId, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var effectiveLanguageId = languageId ?? SupportedLanguages.DefaultLanguageId;

        var result = await Mediator.Send(new GetProblemQuery(userId, id, effectiveLanguageId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("languages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSupportedLanguages(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSupportedLanguagesQuery(), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("problems/{id:guid}/submit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitCode(Guid id, [FromBody] SubmitCodeRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new SubmitCodeCommand(userId, id, request.LanguageId, request.SourceCode),
            cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("problems/{id:guid}/hint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHint(Guid id, [FromBody] GetHintRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetHintCommand(userId, id, request.CurrentCode), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("daily-challenge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDailyChallenge(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDailyCodingChallengeQuery(), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("submissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubmissions([FromQuery] Guid? problemId, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetSubmissionsQuery(userId, problemId), cancellationToken);

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

        var result = await Mediator.Send(new GetCodingStatsQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }
}
