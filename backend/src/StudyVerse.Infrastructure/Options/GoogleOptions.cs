namespace StudyVerse.Infrastructure.Options;

/// <summary>Bound from the "Google" configuration section.</summary>
public sealed class GoogleOptions
{
    public const string SectionName = "Google";

    /// <summary>The OAuth client ID(s) issued by Google Cloud Console that the ID token's "aud" claim must match.</summary>
    public string ClientId { get; set; } = string.Empty;
}
