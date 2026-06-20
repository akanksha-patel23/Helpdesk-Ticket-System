namespace HelpdeskTicket.Core.Settings;

// ─────────────────────────────────────────────────────────────
//  FileUploadSettings
//  Bound from appsettings.json → "FileUploadSettings"
// ─────────────────────────────────────────────────────────────
public sealed class FileUploadSettings
{
    public const string SectionName = "FileUploadSettings";

    // Max allowed upload size in bytes (5 MB = 5,242,880)
    public long MaxFileSizeBytes { get; init; } = 5_242_880;

    // Spec: PDF, PNG, JPEG, JPG only
    public string[] AllowedExtensions { get; init; } = [".pdf", ".png", ".jpeg", ".jpg"];

    // Folder inside wwwroot where files are stored
    public string UploadFolder { get; init; } = "uploads";

    // If set, absolute physical path where uploads are stored (overrides saving inside API wwwroot/uploads)
    public string PhysicalUploadPath { get; init; } = string.Empty;

}
