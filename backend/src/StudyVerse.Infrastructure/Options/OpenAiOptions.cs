namespace StudyVerse.Infrastructure.Options;

/// <summary>
/// Bound from the "OpenAI" configuration section. <see cref="ApiKey"/> is never checked into
/// source: in Development it comes from `dotnet user-secrets`, in Staging/Production from the
/// <c>OpenAI__ApiKey</c> environment variable.
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;
}
