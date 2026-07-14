namespace StudyVerse.Api.Contracts;

/// <summary>Consistent error body shape for Result-pattern failures surfaced by controllers.</summary>
public sealed record ApiErrorResponse(string Error, object? Details = null);
