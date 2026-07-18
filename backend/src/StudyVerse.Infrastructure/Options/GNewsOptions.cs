namespace StudyVerse.Infrastructure.Options;

/// <summary>
/// Bound from the "GNews" configuration section, the same way <see cref="OpenAiOptions"/> is bound
/// from "OpenAI". <see cref="ApiKey"/> is never checked into source: in Development it comes from
/// `dotnet user-secrets`, in Staging/Production from the <c>GNews__ApiKey</c> environment variable.
/// </summary>
public sealed class GNewsOptions
{
    public const string SectionName = "GNews";

    public string ApiKey { get; set; } = string.Empty;
}
