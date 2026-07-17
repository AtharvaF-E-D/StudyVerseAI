using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.Tutor.CreateConversation;
using StudyVerse.Application.Features.Tutor.DeleteConversation;
using StudyVerse.Application.Features.Tutor.GetAiUsage;
using StudyVerse.Application.Features.Tutor.GetConversationMessages;
using StudyVerse.Application.Features.Tutor.GetConversations;
using StudyVerse.Application.Features.Tutor.SendMessage;
using StudyVerse.Application.Features.Tutor.ToggleBookmark;

namespace StudyVerse.Api.Controllers;

/// <summary>
/// The AI tutor: conversations + messages. Note: <see cref="SendMessage"/> returns the complete
/// assembled reply once the OpenAI call finishes — it does NOT stream tokens back to the client.
/// Token-by-token streaming needs its own client/server infrastructure (chunked HTTP responses,
/// React Native-side incremental rendering) and was cut from this pass as a separate, riskier
/// piece of work.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tutor")]
[Authorize]
public sealed class TutorController : ApiControllerBase
{
    [HttpPost("conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateConversation(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new CreateConversationCommand(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("conversations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConversations(
        [FromQuery] string? search,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var effectiveTake = take <= 0 ? 20 : take;

        var result = await Mediator.Send(new GetConversationsQuery(userId, search, effectiveTake), cancellationToken);

        return FromResult(result, conversations => Ok(conversations));
    }

    [HttpGet("conversations/{id:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetConversationMessagesQuery(userId, id), cancellationToken);

        return FromResult(result, messages => Ok(messages));
    }

    [HttpPost("conversations/{id:guid}/messages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new SendMessageCommand(userId, id, request.Content), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpDelete("conversations/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConversation(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new DeleteConversationCommand(userId, id), cancellationToken);

        return FromResult(result, () => NoContent());
    }

    [HttpPost("conversations/{id:guid}/bookmark")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleBookmark(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new ToggleBookmarkCommand(userId, id), cancellationToken);

        return FromResult(result, isBookmarked => Ok(new { isBookmarked }));
    }

    [HttpGet("usage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsage(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetAiUsageQuery(userId), cancellationToken);

        return FromResult(result, usage => Ok(usage));
    }
}
