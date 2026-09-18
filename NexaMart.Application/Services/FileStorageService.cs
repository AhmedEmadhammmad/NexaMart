using System.Security;
using Microsoft.AspNetCore.Hosting;
using NexaMart.Application.Interfaces.Services;

namespace NexaMart.Application.Services;

/// <summary>
/// Secure application file storage service with binary signature validation and isolated folder storage.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveImageAsync(Stream fileStream, string originalFileName, string folderName, CancellationToken cancellationToken = default)
    {
        // 1. Verify file stream is valid and within size limit
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("Uploaded file stream is empty.");
        }

        if (fileStream.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("The image exceeds the maximum permitted file size of 5 MB.");
        }

        // 2. Validate file extension against strict whitelist
        var extension = Path.GetExtension(originalFileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new SecurityException("Invalid file format. Only JPG, PNG, and WebP images are permitted.");
        }

        // 3. Binary Magic Bytes Validation
        ValidateImageMagicBytes(fileStream, extension);

        // 4. Sanitize subfolder name to prevent path traversal
        var sanitizedFolder = folderName.Replace("..", string.Empty).Replace("/", string.Empty).Replace("\\", string.Empty);
        var targetDirectory = Path.Combine(_environment.WebRootPath, "uploads", sanitizedFolder);
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // 5. Generate secure random filename using GUID
        var safeFileName = $"{Guid.NewGuid():N}{extension}";
        var fullDestinationPath = Path.Combine(targetDirectory, safeFileName);

        // 6. Save file stream
        fileStream.Position = 0;
        using (var output = new FileStream(fullDestinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(output, cancellationToken);
        }

        return $"/uploads/{sanitizedFolder}/{safeFileName}";
    }

    public Task<bool> DeleteImageAsync(string? relativeFilePath)
    {
        if (string.IsNullOrWhiteSpace(relativeFilePath) || !relativeFilePath.StartsWith("/uploads/"))
        {
            return Task.FromResult(false);
        }

        try
        {
            var sanitizedRelative = relativeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, sanitizedRelative);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
        }
        catch
        {
            // Silently suppress deletion error to avoid blocking transaction flow
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// Inspects the file header bytes to guarantee that the content matches authentic image signatures.
    /// </summary>
    private static void ValidateImageMagicBytes(Stream stream, string extension)
    {
        stream.Position = 0;
        using var reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen: true);
        var header = reader.ReadBytes(16);
        stream.Position = 0;

        if (header.Length < 4)
        {
            throw new SecurityException("File header is corrupt or truncated.");
        }

        bool isValid = false;

        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                // JPEG magic bytes: FF D8 FF
                isValid = header.Length >= 3 &&
                          header[0] == 0xFF &&
                          header[1] == 0xD8 &&
                          header[2] == 0xFF;
                break;

            case ".png":
                // PNG magic bytes: 89 50 4E 47 0D 0A 1A 0A
                isValid = header.Length >= 8 &&
                          header[0] == 0x89 && header[1] == 0x50 &&
                          header[2] == 0x4E && header[3] == 0x47 &&
                          header[4] == 0x0D && header[5] == 0x0A &&
                          header[6] == 0x1A && header[7] == 0x0A;
                break;

            case ".webp":
                // WebP: RIFF....WEBP (Bytes 0-3 = "RIFF", Bytes 8-11 = "WEBP")
                isValid = header.Length >= 12 &&
                          header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                          header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
                break;
        }

        if (!isValid)
        {
            throw new SecurityException("Security Validation Failed: The file binary content does not match genuine image signatures.");
        }
    }
}
