namespace HelpdeskTicket.Core.Settings;

// ─────────────────────────────────────────────────────────────
//  PaginationSettings
//  Bound from appsettings.json → "PaginationSettings"
//  Spec: pageNumber default = 1, pageSize default = 10
// ─────────────────────────────────────────────────────────────
public sealed class PaginationSettings
{
    public const string SectionName = "PaginationSettings";

    public int DefaultPageNumber { get; init; } = 1;
    public int DefaultPageSize   { get; init; } = 10;
    public int MaxPageSize       { get; init; } = 100;
}
