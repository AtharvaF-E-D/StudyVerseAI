namespace StudyVerse.Application.Common.Models;

/// <summary>
/// Bound from the "AI" configuration section. Lives in Application (like <see cref="JwtOptions"/>)
/// rather than Infrastructure because <c>SendMessageCommandHandler</c> and <c>GetAiUsageQueryHandler</c>
/// need <see cref="DailyTokenLimit"/> directly; the OpenAI-specific API key lives separately in
/// Infrastructure's <c>OpenAiOptions</c>, which Application never references.
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "AI";

    /// <summary>The OpenAI chat model to call, e.g. "gpt-4o-mini".</summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Maximum combined prompt+completion tokens a user may spend on the AI tutor per UTC calendar day.</summary>
    public int DailyTokenLimit { get; set; } = 50_000;
}
