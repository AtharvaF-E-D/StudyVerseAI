namespace StudyVerse.Infrastructure.Options;

/// <summary>
/// Bound from the "Storage" configuration section. Consulted only by
/// <see cref="StudyVerse.Infrastructure.Files.LocalFileStorageService"/> — a future cloud-storage
/// <c>IFileStorageService</c> implementation (Cloudflare R2/S3) would have its own options class
/// instead of reusing this one.
/// </summary>
public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Directory uploaded files are saved under, relative to the process's working
    /// directory unless given as an absolute path. Created automatically if missing.</summary>
    public string LocalRootPath { get; set; } = "App_Data/uploads";
}
