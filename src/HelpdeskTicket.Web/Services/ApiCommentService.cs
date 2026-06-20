using HelpdeskTicket.Application.DTOs.Comment;
using HelpdeskTicket.Core.Wrappers;
using HelpdeskTicket.Web.Helpers;
using System.Text;
using System.Text.Json;

namespace HelpdeskTicket.Web.Services;

// TODO: Implement in step-by-step coding phase
public class ApiCommentService {
    private readonly HttpClient _http;
    private readonly ILogger<ApiCommentService> _logger;
    private readonly IHttpContextAccessor _httpContext;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiCommentService(
        IHttpClientFactory factory,
        ILogger<ApiCommentService> logger,
        IHttpContextAccessor httpContext)
    {
        _http = factory.CreateClient("HelpdeskAPI");
        _logger = logger;
        _httpContext = httpContext;
    }

    public async Task<(bool Success, string Message)> AddCommentAsync(int ticketId, string body)
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var userId = SessionHelper.GetUserId(session);

            var request = new AddCommentRequest
            {
                TicketId = ticketId,
                Body = body,
                CreatedById = userId
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/comments", content);
            var respBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(respBody, _jsonOpts);

            return (result?.Success ?? false, result?.Message ?? "Unknown error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddCommentAsync failed TicketId:{Id}", ticketId);
            return (false, "An unexpected error occurred. Please try again.");
        }
    }
}
