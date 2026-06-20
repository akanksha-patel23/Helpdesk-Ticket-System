using HelpdeskTicket.Application.DTOs.User;
using HelpdeskTicket.Core.Wrappers;
using HelpdeskTicket.Web.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;

namespace HelpdeskTicket.Web.Services;

// TODO: Implement in step-by-step coding phase
public class ApiUserService {
    private readonly HttpClient _http;
    private readonly ILogger<ApiUserService> _logger;
    private readonly IHttpContextAccessor _httpContext;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiUserService(
        IHttpClientFactory factory,
        ILogger<ApiUserService> logger,
        IHttpContextAccessor httpContext)
    {
        _http = factory.CreateClient("HelpdeskAPI");
        _logger = logger;
        _httpContext = httpContext;
    }

    public async Task<List<UserDto>> GetUserListAsync()
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var roleId = SessionHelper.GetRoleId(session);
            var userId = SessionHelper.GetUserId(session);

            var response = await _http.GetAsync(
                $"api/users?requestedRoleId={roleId}&requestedById={userId}");
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<List<UserDto>>>(body, _jsonOpts);
            return result?.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserListAsync failed.");
            return [];
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        try
        {
            var response = await _http.GetAsync($"api/users/{userId}");
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<UserDto>>(body, _jsonOpts);
            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserByIdAsync failed UserId:{Id}", userId);
            return null;
        }
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/users", content);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
            return (result?.Success ?? false, result?.Message ?? "Unknown error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateUserAsync failed.");
            return (false, "An unexpected error occurred.");
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(UpdateUserRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PutAsync($"api/users/{request.UserId}", content);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
            return (result?.Success ?? false, result?.Message ?? "Unknown error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateUserAsync failed UserId:{Id}", request.UserId);
            return (false, "An unexpected error occurred.");
        }
    }
    public async Task<List<SelectListItem>> GetDeveloperSelectListAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/users/developers");
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<List<UserDto>>>(body, _jsonOpts);
            return result?.Data?
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.FullName })
                .ToList() ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "GetDeveloperSelectListAsync failed."); return []; }
    }
}
