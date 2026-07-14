using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudyVerse.Api.Contracts;
using StudyVerse.Domain.Common;

namespace StudyVerse.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>Maps a value-carrying <see cref="Result{T}"/> to an HTTP response.</summary>
    protected IActionResult FromResult<T>(Result<T> result, Func<T, IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : MapFailure(result);

    /// <summary>Maps a non-value <see cref="Result"/> to an HTTP response.</summary>
    protected IActionResult FromResult(Result result, Func<IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess() : MapFailure(result);

    private static IActionResult MapFailure(Result result)
    {
        var statusCode = result.ErrorType switch
        {
            ResultErrorType.Validation => StatusCodes.Status400BadRequest,
            ResultErrorType.NotFound => StatusCodes.Status404NotFound,
            ResultErrorType.Conflict => StatusCodes.Status409Conflict,
            ResultErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ResultErrorType.Locked => StatusCodes.Status423Locked,
            ResultErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ResultErrorType.RateLimited => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status400BadRequest,
        };

        return new ObjectResult(new ApiErrorResponse(result.Error!))
        {
            StatusCode = statusCode,
        };
    }
}
