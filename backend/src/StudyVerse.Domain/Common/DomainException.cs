namespace StudyVerse.Domain.Common;

/// <summary>
/// Thrown only for truly exceptional, programmer-error-like domain invariant violations
/// (e.g. an entity reaching an impossible state). Expected failure paths (bad credentials,
/// duplicate email, expired token, ...) must use <see cref="Result"/> instead.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
