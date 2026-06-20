using System.Net;
using System.Text.Json;
using HelpdeskTicket.Core.Wrappers;

namespace HelpdeskTicket.API.Middleware;

// ─────────────────────────────────────────────────────────────
//  GlobalExceptionMiddleware
//
//  Catches ALL unhandled exceptions across the API pipeline.
//  Registered FIRST in Program.cs so it wraps everything.
//
//  Behaviour:
//  • Logs full exception (stack trace, message) via ILogger
//  • Returns HTTP 500 + ApiResponse.Fail() JSON to client
//  • Hides internal detail in Production; exposes it in Development
// ─────────────────────────────────────────────────────────────
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate  _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(
            exception,
            "Unhandled exception. Path: {Path} | Method: {Method} | TraceId: {TraceId}",
            context.Request.Path,
            context.Request.Method,
            context.TraceIdentifier);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

        var message = _env.IsDevelopment()
            ? $"{exception.GetType().Name}: {exception.Message}"
            : "An unexpected error occurred. Please try again or contact support.";

        var response = ApiResponse.Fail(message);

        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = null });

        await context.Response.WriteAsync(json);
    }
}
