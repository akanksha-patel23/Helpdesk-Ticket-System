namespace HelpdeskTicket.Core.Wrappers;

// ─────────────────────────────────────────────────────────────
//  ApiResponse<T>
//  Generic response envelope returned by every API endpoint.
//
//  JSON shape (as per spec):
//  {
//    "success": true,
//    "message": "Ticket created",
//    "data": { ... }
//  }
// ─────────────────────────────────────────────────────────────
public sealed class ApiResponse<T>
{
    public bool   Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T?     Data    { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new() { Success = true,  Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message, Data = default };
}

// ─────────────────────────────────────────────────────────────
//  ApiResponse  (non-generic — for operations returning no data)
// ─────────────────────────────────────────────────────────────
public sealed class ApiResponse
{
    public bool   Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ApiResponse Ok(string message = "Success")
        => new() { Success = true,  Message = message };

    public static ApiResponse Fail(string message)
        => new() { Success = false, Message = message };
}
