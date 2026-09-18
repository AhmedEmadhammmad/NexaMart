namespace NexaMart.Application.Interfaces.Services;

/// <summary>
/// Service contract for secure file uploads, binary validation, and file deletion.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Validates and saves an image stream into the designated folder after verifying binary signatures.
    /// </summary>
    /// <param name="fileStream">Stream of the uploaded file.</param>
    /// <param name="originalFileName">Original client filename for extension extraction.</param>
    /// <param name="folderName">Target subfolder under uploads (e.g. products, categories).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The relative web URL to the saved image (e.g. /uploads/products/xyz.webp).</returns>
    Task<string> SaveImageAsync(Stream fileStream, string originalFileName, string folderName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image from the local storage if it exists.
    /// </summary>
    /// <param name="relativeFilePath">The relative web path of the image.</param>
    /// <returns>True if deleted, otherwise false.</returns>
    Task<bool> DeleteImageAsync(string? relativeFilePath);
}
