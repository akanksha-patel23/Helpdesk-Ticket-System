using HelpdeskTicket.Application.DTOs.Ticket;
using HelpdeskTicket.Core.Wrappers;
using HelpdeskTicket.Web.Helpers;
using HelpdeskTicket.Web.ViewModels.Ticket;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace HelpdeskTicket.Web.Services;

// TODO: Implement in step-by-step coding phase
public class ApiTicketService
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiTicketService> _logger;
    private readonly IHttpContextAccessor _httpContext;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiTicketService(
        IHttpClientFactory factory,
        ILogger<ApiTicketService> logger,
        IHttpContextAccessor httpContext)
    {
        _http = factory.CreateClient("HelpdeskAPI");
        _logger = logger;
        _httpContext = httpContext;
    }

    public async Task<TicketDetailDto?> GetTicketByIdAsync(int ticketId)
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var userId = SessionHelper.GetUserId(session);
            var roleId = SessionHelper.GetRoleId(session);

            var url = $"api/tickets/{ticketId}?userId={userId}&roleId={roleId}";
            var response = await _http.GetAsync($"api/tickets/{ticketId}?userId={userId}&roleId={roleId}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Log full details to diagnose 400/401/404 and body returned by API
                _logger.LogWarning("GetTicketByIdAsync failed. Url:{Url} StatusCode:{Status} UserId:{UserId} RoleId:{RoleId} Body:{Body}",
                    url, response.StatusCode, userId, roleId, body);
                return null;
            }

            var result = JsonSerializer.Deserialize<ApiResponse<TicketDetailDto>>(body, _jsonOpts);
            if (result is null)
            {
                _logger.LogWarning("GetTicketByIdAsync deserialization returned null. Url:{Url} Body:{Body}", url, body);
                return null;
            }

            if (!result.Success)
            {
                _logger.LogInformation("GetTicketByIdAsync ApiResponse not successful. Url:{Url} Message:{Message} UserId:{UserId} RoleId:{RoleId}",
                    url, result.Message, userId, roleId);
            }

            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTicketByIdAsync failed TicketId:{Id}", ticketId);
            return null;
        }
    }

    //public async Task<(bool Success, string Message)> UpdateTicketAsync(
    //    int ticketId, int? statusId, int? priorityId, int? assignedToId)
    //{
    //    try
    //    {
    //        var session = _httpContext.HttpContext!.Session;
    //        var userId = SessionHelper.GetUserId(session);
    //        var roleId = SessionHelper.GetRoleId(session);

    //        var request = new UpdateTicketRequest
    //        {
    //            TicketId = ticketId,
    //            StatusId = statusId,
    //            PriorityId = priorityId,
    //            AssignedToId = assignedToId,
    //            UpdatedById = userId,
    //            UpdatedByRoleId = roleId
    //        };

    //        var json = JsonSerializer.Serialize(request);
    //        var content = new StringContent(json, Encoding.UTF8, "application/json");
    //        var response = await _http.PutAsync($"api/tickets/{ticketId}", content);
    //        var body = await response.Content.ReadAsStringAsync();
    //        var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);

    //        return (result?.Success ?? false, result?.Message ?? "Unknown error.");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "UpdateTicketAsync failed TicketId:{Id}", ticketId);
    //        return (false, "An unexpected error occurred.");
    //    }
    //}

    //public async Task<(bool Success, string Message)> UpdateTicketAsync(
    //int ticketId, int? statusId, int? priorityId, int? assignedToId,
    //IFormFile? attachment = null)
    //{
    //    try
    //    {
    //        var session = _httpContext.HttpContext!.Session;
    //        var userId = SessionHelper.GetUserId(session);

    //        using var form = new MultipartFormDataContent();
    //        form.Add(new StringContent(ticketId.ToString()), "TicketId");
    //        form.Add(new StringContent(userId.ToString()), "UpdatedById");

    //        if (statusId.HasValue)
    //            form.Add(new StringContent(statusId.ToString()!), "StatusId");
    //        if (priorityId.HasValue)
    //            form.Add(new StringContent(priorityId.ToString()!), "PriorityId");
    //        if (assignedToId.HasValue)
    //            form.Add(new StringContent(assignedToId.ToString()!), "AssignedToId");

    //        if (attachment is not null && attachment.Length > 0)
    //        {
    //            var stream = attachment.OpenReadStream();
    //            var content = new StreamContent(stream);
    //            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
    //                attachment.ContentType ?? "application/octet-stream");
    //            form.Add(content, "attachment", attachment.FileName);
    //        }

    //        var response = await _http.PutAsync($"api/tickets/{ticketId}", form);
    //        var body = await response.Content.ReadAsStringAsync();
    //        var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
    //        return (result?.Success ?? false, result?.Message ?? "Unknown error.");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "UpdateTicketAsync failed TicketId:{Id}", ticketId);
    //        return (false, "An unexpected error occurred.");
    //    }
    //}
    // Add attachment
    public async Task<(bool Success, string Message)> AddAttachmentAsync(
        int ticketId, IFormFile file)
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var userId = SessionHelper.GetUserId(session);

            using var form = new MultipartFormDataContent();
            var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                file.ContentType ?? "application/octet-stream");
            form.Add(content, "attachment", file.FileName);

            var response = await _http.PostAsync(
                $"api/tickets/{ticketId}/attachments?userId={userId}", form);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
            return (result?.Success ?? false, result?.Message ?? "Unknown error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddAttachmentAsync failed.");
            return (false, "An unexpected error occurred.");
        }
    }

    // Delete attachment
    public async Task<(bool Success, string Message)> DeleteAttachmentAsync(int attachmentId)
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var userId = SessionHelper.GetUserId(session);

            var response = await _http.DeleteAsync(
                $"api/tickets/attachments/{attachmentId}?userId={userId}");
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
            return (result?.Success ?? false, result?.Message ?? "Unknown error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAttachmentAsync failed.");
            return (false, "An unexpected error occurred.");
        }
    }

    // Updated UpdateTicketAsync — sends Title + Description
    public async Task<(bool Success, string Message)> UpdateTicketAsync(
        int ticketId, string? title, string? description,
        int? statusId, int? priorityId, int? assignedToId)
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var userId = SessionHelper.GetUserId(session);
            var roleId = SessionHelper.GetRoleId(session);

            var request = new UpdateTicketRequest
            {
                TicketId = ticketId,
                Title = title,
                Description = description,
                StatusId = statusId,
                PriorityId = priorityId,
                AssignedToId = assignedToId,
                UpdatedById = userId,
                UpdatedByRoleId = roleId
            };

            //var json = JsonSerializer.Serialize(request);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");
            var content = new MultipartFormDataContent();

            content.Add(new StringContent(ticketId.ToString()), "TicketId");

            if (!string.IsNullOrWhiteSpace(title))
                content.Add(new StringContent(title), "Title");

            if (!string.IsNullOrWhiteSpace(description))
                content.Add(new StringContent(description), "Description");

            if (statusId.HasValue)
                content.Add(new StringContent(statusId.Value.ToString()), "StatusId");

            if (priorityId.HasValue)
                content.Add(new StringContent(priorityId.Value.ToString()), "PriorityId");

            if (assignedToId.HasValue)
                content.Add(new StringContent(assignedToId.Value.ToString()), "AssignedToId");

            content.Add(new StringContent(userId.ToString()), "UpdatedById");
            content.Add(new StringContent(roleId.ToString()), "UpdatedByRoleId");

            var response = await _http.PutAsync($"api/tickets/{ticketId}", content);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
            return (result?.Success ?? false, result?.Message ?? "Unknown error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateTicketAsync failed.");
            return (false, "An unexpected error occurred.");
        }
    }

    public async Task<PagedResponse<TicketListDto>?> GetTicketListAsync(TicketListViewModel filter)
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var userId = SessionHelper.GetUserId(session);
            var roleId = SessionHelper.GetRoleId(session);

            var url = $"api/tickets?userId={userId}&roleId={roleId}" +
                      $"&pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
                url += $"&searchText={Uri.EscapeDataString(filter.SearchText)}";
            if (filter.StatusId.HasValue)
                url += $"&statusId={filter.StatusId}";
            if (filter.PriorityId.HasValue)
                url += $"&priorityId={filter.PriorityId}";
            if (filter.CategoryId.HasValue)
                url += $"&categoryId={filter.CategoryId}";

            var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PagedResponse<TicketListDto>>(body, _jsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTicketListAsync failed.");
            return null;
        }
    }

    // Fetch categories + priorities for dropdowns
    public async Task<TicketDropdownsDto?> GetDropdownsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/tickets/dropdowns");
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<TicketDropdownsDto>>(body, _jsonOpts);
            return result?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDropdownsAsync failed.");
            return null;
        }
    }

    // POST ticket as multipart/form-data so file travels with fields
    public async Task<(bool Success, string Message)> CreateTicketAsync(CreateTicketViewModel model)
    {
        try
        {
            var session = _httpContext.HttpContext!.Session;
            var userId = SessionHelper.GetUserId(session);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(model.Title), "Title");
            form.Add(new StringContent(model.Description), "Description");
            form.Add(new StringContent(model.CategoryId.ToString()), "CategoryId");
            form.Add(new StringContent(model.PriorityId.ToString()), "PriorityId");
            form.Add(new StringContent(userId.ToString()), "CreatedById");

            if (model.Attachment is not null && model.Attachment.Length > 0)
            {
                var stream = model.Attachment.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    model.Attachment.ContentType ?? "application/octet-stream");
                form.Add(content, "attachment", model.Attachment.FileName);
            }

            var response = await _http.PostAsync("api/tickets", form);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);

            return (result?.Success ?? false, result?.Message ?? "Unknown error.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateTicketAsync failed.");
            return (false, "An unexpected error occurred. Please try again.");
        }
    }

    //category dropdown
    public async Task<List<SelectListItem>> GetCategorySelectListAsync()
    {
        var dropdowns = await GetDropdownsAsync();
        return dropdowns?.Categories?
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList() ?? [];
    }

    // Converts raw DTO to SelectListItem lists for Razor asp-items
    public async Task<(List<SelectListItem> Categories, List<SelectListItem> Priorities)> GetSelectListsAsync()
    {
        var dropdowns = await GetDropdownsAsync();

        var categories = dropdowns?.Categories
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList() ?? [];

        var priorities = dropdowns?.Priorities
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
            .ToList() ?? [];

        return (categories, priorities);
    }
    // Filter bar: static status list (mirrors tlkpStatus seed data)
    public static List<SelectListItem> GetStatusSelectList() =>
    [
        //new SelectListItem { Value = "1", Text = "Open" },
        //new SelectListItem { Value = "2", Text = "Assigned" },
        //new SelectListItem { Value = "3", Text = "In Progress" },
        //new SelectListItem { Value = "4", Text = "Pending" },
        //new SelectListItem { Value = "5", Text = "Waiting for User" },
        //new SelectListItem { Value = "6", Text = "Resolved" },
        //new SelectListItem { Value = "7", Text = "Closed" },
        //new SelectListItem { Value = "8", Text = "Reopened" },
        //new SelectListItem { Value = "9", Text = "Cancelled" }
        new SelectListItem { Value = "1", Text = "Open" },
        new SelectListItem { Value = "2", Text = "Assigned" },
        new SelectListItem { Value = "3", Text = "In Progress" },
        new SelectListItem { Value = "4", Text = "Closed" },
        new SelectListItem { Value = "5", Text = "Resolved" }
    ];

    // Filter bar: static priority list (mirrors tlkpPriority seed data)
    public static List<SelectListItem> GetPrioritySelectList() =>
    [
        new SelectListItem { Value = "1", Text = "Low" },
        new SelectListItem { Value = "2", Text = "Medium" },
        new SelectListItem { Value = "3", Text = "High" },
        new SelectListItem { Value = "4", Text = "Critical" }
    ];
}
