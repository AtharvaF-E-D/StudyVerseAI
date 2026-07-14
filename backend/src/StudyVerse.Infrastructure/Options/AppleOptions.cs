namespace StudyVerse.Infrastructure.Options;

/// <summary>Bound from the "Apple" configuration section.</summary>
public sealed class AppleOptions
{
    public const string SectionName = "Apple";

    /// <summary>The app's bundle ID / Services ID that the identity token's "aud" claim must match.</summary>
    public string ClientId { get; set; } = string.Empty;
}
