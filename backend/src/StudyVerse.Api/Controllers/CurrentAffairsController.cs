using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Application.Features.CurrentAffairs.GetArticle;
using StudyVerse.Application.Features.CurrentAffairs.GetArticleQuiz;
using StudyVerse.Application.Features.CurrentAffairs.GetArticlesByCategory;
using StudyVerse.Application.Features.CurrentAffairs.GetBookmarks;
using StudyVerse.Application.Features.CurrentAffairs.GetCategories;
using StudyVerse.Application.Features.CurrentAffairs.GetWeeklyDigest;
using StudyVerse.Application.Features.CurrentAffairs.SearchArticles;
using StudyVerse.Application.Features.CurrentAffairs.ToggleBookmark;

namespace StudyVerse.Api.Controllers;

/// <summary>
/// Current Affairs: a category news feed cached from GNews, live search, bookmarks, a per-article
/// AI comprehension quiz, and a shared weekly AI digest.
///
/// <c>[Authorize]</c> applies to the whole controller, including <see cref="GetCategories"/> even
/// though it's static data with no per-user state - matching <c>QuizController.GetCategories</c>
/// (also authorized despite being static), rather than carving out an inconsistent anonymous
/// exception for one endpoint in an otherwise fully-authenticated feature.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/currentaffairs")]
[Authorize]
public sealed class CurrentAffairsController : ApiControllerBase
{
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCategoriesQuery(), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("articles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetArticlesByCategory([FromQuery] string category, [FromQuery] int take, CancellationToken cancellationToken)
    {
        var effectiveTake = take <= 0 ? 10 : take;

        var result = await Mediator.Send(new GetArticlesByCategoryQuery(category, effectiveTake), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new SearchArticlesQuery(q), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("articles/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArticle(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetArticleQuery(id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpPost("articles/{id:guid}/bookmark")]
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

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("bookmarks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBookmarks(CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetBookmarksQuery(userId), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("articles/{id:guid}/quiz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArticleQuiz(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetArticleQuizQuery(id), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }

    [HttpGet("digest/weekly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeeklyDigest(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetWeeklyDigestQuery(), cancellationToken);

        return FromResult(result, dto => Ok(dto));
    }
}
