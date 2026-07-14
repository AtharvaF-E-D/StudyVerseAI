namespace StudyVerse.Domain.Common;

/// <summary>
/// Represents the outcome of an operation that can fail for expected, non-exceptional reasons
/// (e.g. "email already registered", "invalid credentials"). Handlers should return a
/// <see cref="Result"/>/<see cref="Result{T}"/> instead of throwing for these cases; exceptions
/// are reserved for truly unexpected failures.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    protected Result(bool isSuccess, string? error, ResultErrorType errorType)
    {
        if (isSuccess && error is not null)
        {
            throw new InvalidOperationException("A successful result cannot have an error message.");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new InvalidOperationException("A failed result must have an error message.");
        }

        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ResultErrorType.None);

    public static Result Failure(string error, ResultErrorType errorType = ResultErrorType.Validation) =>
        new(false, error, errorType);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    public static Result<T> Failure<T>(string error, ResultErrorType errorType = ResultErrorType.Validation) =>
        Result<T>.Failure(error, errorType);

    /// <summary>
    /// Builds a failure of the exact runtime <typeparamref name="TResult"/> (either <see cref="Result"/>
    /// or some <see cref="Result{T}"/>) without the caller needing to know which. Used by generic
    /// MediatR pipeline behaviors that only know TResponse via a `where TResponse : Result` constraint.
    /// </summary>
    public static TResult FailureAs<TResult>(string error, ResultErrorType errorType = ResultErrorType.Validation)
        where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)(object)Failure(error, errorType);
        }

        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = typeof(TResult).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>).MakeGenericType(valueType)
                .GetMethod(nameof(Failure), [typeof(string), typeof(ResultErrorType)]);

            return (TResult)failureMethod!.Invoke(null, [error, errorType])!;
        }

        throw new InvalidOperationException($"Cannot build a failure result for type '{typeof(TResult)}'.");
    }
}

/// <summary>
/// A <see cref="Result"/> that carries a value on success.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
        : base(isSuccess, error, errorType)
    {
        _value = value;
    }

    /// <summary>
    /// The success value. Throws if accessed on a failed result.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.None);

    public static new Result<T> Failure(string error, ResultErrorType errorType = ResultErrorType.Validation) =>
        new(false, default, error, errorType);
}

/// <summary>
/// Categorizes a failure so the API layer can map it to the appropriate HTTP status code
/// without the Application layer needing to know about HTTP.
/// </summary>
public enum ResultErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Locked = 5,
    Forbidden = 6,
    RateLimited = 7,
}
