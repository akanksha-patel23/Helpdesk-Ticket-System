namespace HelpdeskTicket.Core.Wrappers;

// ─────────────────────────────────────────────────────────────
//  PagedResponse<T>
//  Wraps paginated list results with metadata.
//  TotalPages, HasPrevious, HasNext are computed — not stored.
// ─────────────────────────────────────────────────────────────
public sealed class PagedResponse<T>
{
    public bool      Success    { get; init; }
    public string    Message    { get; init; } = string.Empty;
    public IList<T>  Data       { get; init; } = [];
    public int       PageNumber { get; init; }
    public int       PageSize   { get; init; }
    public int       TotalCount { get; init; }

    public int  TotalPages  => TotalCount == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext     => PageNumber < TotalPages;

    public static PagedResponse<T> Ok(
        IList<T> data,
        int totalCount,
        int pageNumber,
        int pageSize,
        string message = "Success")
        => new()
        {
            Success    = true,
            Message    = message,
            Data       = data,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize   = pageSize
        };

    public static PagedResponse<T> Fail(string message)
        => new() { Success = false, Message = message };
}
