using HelpdeskTicket.Application.DTOs.Category;
using HelpdeskTicket.Core.Wrappers;
using System.Text;
using System.Text.Json;

namespace HelpdeskTicket.Web.Services;

public class ApiCategoryService
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiCategoryService> _logger;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiCategoryService(IHttpClientFactory factory, ILogger<ApiCategoryService> logger)
    {
        _http = factory.CreateClient("HelpdeskAPI");
        _logger = logger;
    }

    // includeInactive: false = ticket form, true = admin management page
    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive = false)
    {
        try
        {
            var response = await _http.GetAsync($"api/categories?includeInactive={includeInactive}");
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(body, _jsonOpts);
            return result?.Data ?? [];
        }
        catch (Exception ex) { _logger.LogError(ex, "GetAllAsync categories failed."); return []; }
    }

    public async Task<(bool Success, string Message)> SaveAsync(SaveCategoryRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/categories", content);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
            return (result?.Success ?? false, result?.Message ?? "Unknown error.");
        }
        catch (Exception ex) { _logger.LogError(ex, "SaveAsync category failed."); return (false, "An unexpected error occurred."); }
    }
    //public async Task<(bool Success, string Message)> DeleteAsync(int id)
    //{
    //    try
    //    {
    //        var response = await _http.DeleteAsync($"api/categories/{id}");
    //        var body = await response.Content.ReadAsStringAsync();
    //        var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
    //        return (result?.Success ?? false, result?.Message ?? "Unknown error.");
    //    }
    //    catch (Exception ex) { _logger.LogError(ex, "DeleteAsync category failed Id:{Id}", id); return (false, "An unexpected error occurred."); }
    //}
}
