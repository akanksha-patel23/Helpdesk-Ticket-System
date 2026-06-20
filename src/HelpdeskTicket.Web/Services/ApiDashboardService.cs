using HelpdeskTicket.Application.DTOs.Dashboard;
using HelpdeskTicket.Core.Wrappers;
using HelpdeskTicket.Web.Helpers;
using System.Text.Json;

namespace HelpdeskTicket.Web.Services;

public class ApiDashboardService
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiDashboardService> _logger;
    private readonly IHttpContextAccessor _httpContext;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiDashboardService(
        IHttpClientFactory factory,
        ILogger<ApiDashboardService> logger,
        IHttpContextAccessor httpContext)
    {
        _http = factory.CreateClient("HelpdeskAPI");
        _logger = logger;
        _httpContext = httpContext;
    }

    public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var userId = SessionHelper.GetUserId(session);
            var roleId = SessionHelper.GetRoleId(session);

            var response = await _http.GetAsync($"api/dashboard?userId={userId}&roleId={roleId}");
            var body = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ApiResponse<DashboardStatsDto>>(body, _jsonOpts);
            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApiDashboardService.GetDashboardStatsAsync failed.");
            return null;
        }
    }
}
