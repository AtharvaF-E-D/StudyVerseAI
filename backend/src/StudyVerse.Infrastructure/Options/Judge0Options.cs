namespace StudyVerse.Infrastructure.Options;

/// <summary>
/// Bound from the "Judge0" configuration section, the same way <see cref="GNewsOptions"/> is bound
/// from "GNews". <see cref="RapidApiKey"/> is never checked into source: in Development it comes
/// from `dotnet user-secrets`, in Staging/Production from the <c>Judge0__RapidApiKey</c>
/// environment variable. The host/base URL are not configurable - they're the fixed real Judge0 CE
/// RapidAPI endpoint, hardcoded alongside the <c>HttpClient</c> registration in
/// <c>DependencyInjection</c> (same as GNews's base address).
/// </summary>
public sealed class Judge0Options
{
    public const string SectionName = "Judge0";

    public string RapidApiKey { get; set; } = string.Empty;
}
