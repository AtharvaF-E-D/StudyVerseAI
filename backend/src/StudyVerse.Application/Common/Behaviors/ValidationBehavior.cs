using FluentValidation;
using MediatR;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Common.Behaviors;

/// <summary>
/// Runs all registered FluentValidation validators for <typeparamref name="TRequest"/> before the
/// handler executes. On failure, short-circuits the pipeline and returns a failed
/// <see cref="Result"/>/<see cref="Result{T}"/> instead of throwing.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var errorMessage = string.Join(" | ", failures.Select(f => f.ErrorMessage).Distinct());
            return Result.FailureAs<TResponse>(errorMessage, ResultErrorType.Validation);
        }

        return await next(cancellationToken);
    }
}
