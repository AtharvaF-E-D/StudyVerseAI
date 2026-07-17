using Microsoft.Extensions.Options;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Infrastructure.Options;

namespace StudyVerse.Infrastructure.Files;

/// <summary>
/// Stores note uploads as ordinary files under a configured root directory
/// (<see cref="StorageOptions.LocalRootPath"/>, created on first use if missing) — a fully working
/// implementation for local development and small deployments, not a placeholder. The storage key
/// handed back to callers is just a generated file name; callers must treat it as opaque (see
/// <see cref="IFileStorageService"/>'s doc comment).
///
/// Swapping to Cloudflare R2/S3 later means writing a new <see cref="IFileStorageService"/>
/// (e.g. <c>R2FileStorageService</c> using the AWS S3-compatible SDK) and changing one line in
/// <c>DependencyInjection.AddInfrastructure</c> — no cloud-specific code belongs in this class.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IOptions<StorageOptions> options)
    {
        _rootPath = Path.GetFullPath(options.Value.LocalRootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        // Guid-named on disk (not the original file name) so uploads can never collide or path-
        // traverse via a hostile file name; SourceFileName on the Note entity keeps the original
        // for display.
        var storageKey = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(_rootPath, storageKey);

        await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return storageKey;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storageKey);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>Resolves a storage key to a full path, rejecting anything that would escape the
    /// storage root (defense in depth — storage keys are always our own Guid-based names, but a
    /// future caller passing one through unchecked shouldn't be able to read/delete arbitrary
    /// files on disk).</summary>
    private string ResolvePath(string storageKey)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storageKey));
        if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid storage key.");
        }

        return fullPath;
    }
}
