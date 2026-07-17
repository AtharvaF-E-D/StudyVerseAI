namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Abstraction over wherever uploaded note files (PDF/DOCX/image) physically live, mirroring the
/// <c>IEmailSender</c>/<c>IOtpSender</c> split: interface here, provider-specific implementation in
/// Infrastructure. <see cref="LocalFileStorageService"/> stores files on local disk for now — a
/// fully working implementation, not a stub. Swapping to Cloudflare R2/S3 later just means writing
/// a new <see cref="IFileStorageService"/> implementation and changing one DI registration; no
/// Application-layer code (UploadNoteCommandHandler, DeleteNoteCommandHandler) needs to change.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Persists <paramref name="content"/> and returns an opaque storage key that later
    /// identifies it. Callers must not assume any particular structure for the key (e.g. that it's
    /// a file path) — only that it can be round-tripped through <see cref="OpenReadAsync"/> and
    /// <see cref="DeleteAsync"/>.</summary>
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
