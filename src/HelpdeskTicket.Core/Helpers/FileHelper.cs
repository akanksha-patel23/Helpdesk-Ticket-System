namespace HelpdeskTicket.Core.Helpers;

// ─────────────────────────────────────────────────────────────
//  FileHelper
//  Static utility methods for file upload validation.
//  Used by both API (server-side) and Web (client-side hints).
//  No IO operations here — pure validation logic only.
// ─────────────────────────────────────────────────────────────
public static class FileHelper
{
    // Spec: PDF, PNG, JPEG, JPG strictly
    private static readonly string[] _allowedExtensions = [".pdf", ".png", ".jpeg", ".jpg"];

    // Spec: max 5 MB
    private const long MaxFileSizeBytes = 5_242_880;

    /// <summary>Returns true if the file extension is in the allowed list.</summary>
    public static bool IsAllowedExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(ext);
    }

    /// <summary>Returns true if the file size is within the 5 MB limit.</summary>
    public static bool IsAllowedSize(long fileSizeBytes) => fileSizeBytes <= MaxFileSizeBytes;

    /// <summary>
    /// Generates a unique file name to prevent collisions in wwwroot/uploads.
    /// Format: {originalName_withoutExt}_{guid}{ext}
    /// Example: screenshot_3f2a1b.png
    /// </summary>
    public static string GenerateUniqueFileName(string originalFileName)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var guid = Guid.NewGuid().ToString("N")[..8];   // short 8-char hex suffix
        return $"{baseName}_{guid}{ext}";
    }

    /// <summary>Returns the virtual path stored in the DB (relative to wwwroot).</summary>
    public static string GetVirtualPath(string uploadFolder, string uniqueFileName)
        => $"/{uploadFolder}/{uniqueFileName}";
}
