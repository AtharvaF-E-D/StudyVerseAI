using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.Flashcards.AddCard;
using StudyVerse.Application.Features.Flashcards.CreateDeck;
using StudyVerse.Application.Features.Flashcards.DeleteCard;
using StudyVerse.Application.Features.Flashcards.DeleteDeck;
using StudyVerse.Application.Features.Flashcards.GenerateDeckFromNote;
using StudyVerse.Application.Features.Flashcards.GenerateDeckFromTopic;
using StudyVerse.Application.Features.Flashcards.GetDeck;
using StudyVerse.Application.Features.Flashcards.GetDecks;
using StudyVerse.Application.Features.Flashcards.GetDueCards;
using StudyVerse.Application.Features.Flashcards.GetFlashcardStats;
using StudyVerse.Application.Features.Flashcards.GetSharedDeck;
using StudyVerse.Application.Features.Flashcards.ReviewCard;
using StudyVerse.Application.Features.Flashcards.ShareDeck;
using StudyVerse.Application.Features.Flashcards.ToggleDeckFavorite;
using StudyVerse.Application.Features.Flashcards.UnshareDeck;
using StudyVerse.Application.Features.Flashcards.UpdateCard;

namespace StudyVerse.Api.Controllers;

/// <summary>
/// Flashcards: manual/AI-generated/note-derived decks, real SM-2 spaced-repetition scheduling
/// (<c>Domain.SpacedRepetition.Sm2Scheduler</c>), a cross-deck daily due-cards review queue, and
/// public read-only deck sharing by token. Every action requires auth except
/// <see cref="GetSharedDeck"/>, which is deliberately anonymous — see that action's attribute.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/flashcards")]
[Authorize]
public sealed class FlashcardsController : ApiControllerBase
{
    [HttpPost("decks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateDeck([FromBody] CreateDeckRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new CreateDeckCommand(userId, request.Title, request.Description), cancellationToken);

        return FromResult(result, id => Ok(new { id }));
    }

    [HttpPost("decks/from-topic")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateDeckFromTopic(
        [FromBody] GenerateDeckFromTopicRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new GenerateDeckFromTopicCommand(userId, request.Title, request.Topic, request.CardCount),
            cancellationToken);

        return FromResult(result, id => Ok(new { id }));
    }

    [HttpPost("decks/from-note/{noteId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateDeckFromNote(Guid noteId, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GenerateDeckFromNoteCommand(userId, noteId), cancellationToken);

        return FromResult(result, id => Ok(new { id }));
    }

    [HttpGet("decks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDecks(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetDecksQuery(userId), cancellationToken);

        return FromResult(result, decks => Ok(decks));
    }

    [HttpGet("decks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDeck(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetDeckQuery(userId, id), cancellationToken);

        return FromResult(result, deck => Ok(deck));
    }

    [HttpDelete("decks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDeck(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new DeleteDeckCommand(userId, id), cancellationToken);

        return FromResult(result, () => NoContent());
    }

    [HttpPost("decks/{id:guid}/favorite")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new ToggleDeckFavoriteCommand(userId, id), cancellationToken);

        return FromResult(result, isFavorite => Ok(new { isFavorite }));
    }

    [HttpPost("decks/{id:guid}/share")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ShareDeck(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new ShareDeckCommand(userId, id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpDelete("decks/{id:guid}/share")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnshareDeck(Guid id, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new UnshareDeckCommand(userId, id), cancellationToken);

        return FromResult(result, () => NoContent());
    }

    /// <summary>The one anonymous endpoint on this controller — a public read-only view of a
    /// shared deck by its share token, no bearer token required or checked at all.</summary>
    [HttpGet("shared/{shareToken}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSharedDeck(string shareToken, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSharedDeckQuery(shareToken), cancellationToken);

        return FromResult(result, deck => Ok(deck));
    }

    [HttpPost("decks/{id:guid}/cards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCard(Guid id, [FromBody] AddCardRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new AddCardCommand(userId, id, request.FrontText, request.BackText, request.ImageUrl),
            cancellationToken);

        return FromResult(result, cardId => Ok(new { id = cardId }));
    }

    [HttpPut("decks/{deckId:guid}/cards/{cardId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCard(
        Guid deckId, Guid cardId, [FromBody] UpdateCardRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new UpdateCardCommand(userId, deckId, cardId, request.FrontText, request.BackText, request.ImageUrl),
            cancellationToken);

        return FromResult(result, () => NoContent());
    }

    [HttpDelete("decks/{deckId:guid}/cards/{cardId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCard(Guid deckId, Guid cardId, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new DeleteCardCommand(userId, deckId, cardId), cancellationToken);

        return FromResult(result, () => NoContent());
    }

    [HttpGet("due")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDueCards([FromQuery] Guid? deckId, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetDueCardsQuery(userId, deckId), cancellationToken);

        return FromResult(result, cards => Ok(cards));
    }

    [HttpPost("cards/{id:guid}/review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReviewCard(Guid id, [FromBody] ReviewCardRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new ReviewCardCommand(userId, id, request.Quality), cancellationToken);

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

        var result = await Mediator.Send(new GetFlashcardStatsQuery(userId), cancellationToken);

        return FromResult(result, stats => Ok(stats));
    }
}
