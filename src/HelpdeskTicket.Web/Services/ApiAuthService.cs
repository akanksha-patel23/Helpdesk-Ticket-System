using HelpdeskTicket.Application.DTOs.Auth;
using HelpdeskTicket.Application.DTOs.User;
using HelpdeskTicket.Core.Wrappers;
using System.Text;
using System.Text.Json;

namespace HelpdeskTicket.Web.Services;

public class ApiAuthService
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiAuthService> _logger;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiAuthService(IHttpClientFactory factory, ILogger<ApiAuthService> logger)
    {
        _http = factory.CreateClient("HelpdeskAPI");
        _logger = logger;
    }
    public async Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/auth/login", content);
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(body, _jsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApiAuthService.LoginAsync failed.");
            return null;
        }
    }

    public async Task<ApiResponse<object>?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/auth/register", content);
            var body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApiAuthService.RegisterAsync failed.");
            return null;
        }
    }

    //public async Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request)
    //{
    //    try
    //    {
    //        var requestJson = JsonSerializer.Serialize(request);
    //        _logger.LogDebug("LoginAsync: POST api/auth/login. Payload: {Payload}", requestJson);

    //        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
    //        var response = await _http.PostAsync("api/auth/login", content);
    //        var body = await response.Content.ReadAsStringAsync();

    //        _logger.LogInformation("LoginAsync: API status {StatusCode}. Body: {Body}", response.StatusCode, body);

    //        // Try deserialize even when non-success so controller receives the API message
    //        try
    //        {
    //            return JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(body, _jsonOpts);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "LoginAsync: Failed to deserialize API response body.");
    //            return ApiResponse<LoginResponse>.Fail("Invalid response from API.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "ApiAuthService.LoginAsync failed.");
    //        return ApiResponse<LoginResponse>.Fail("Communication error with API.");
    //    }
    //}

    //public async Task<ApiResponse<object>?> RegisterAsync(RegisterRequest request)
    //{
    //    try
    //    {
    //        var requestJson = JsonSerializer.Serialize(request);
    //        _logger.LogDebug("RegisterAsync: POST api/auth/register. Payload: {Payload}", requestJson);

    //        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
    //        var response = await _http.PostAsync("api/auth/register", content);
    //        var body = await response.Content.ReadAsStringAsync();

    //        _logger.LogInformation("RegisterAsync: API status {StatusCode}. Body: {Body}", response.StatusCode, body);

    //        // Attempt to deserialize API response (works for success and failure payloads)
    //        try
    //        {
    //            return JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "RegisterAsync: Failed to deserialize API response body.");
    //            return ApiResponse<object>.Fail("Invalid response from API.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "ApiAuthService.RegisterAsync failed.");
    //        return ApiResponse<object>.Fail("Communication error with API.");
    //    }
    //}


    //public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    //{
    //    try
    //    {
    //        var request = new ResetPasswordRequest { Email = email, NewPassword = newPassword };
    //        var json = JsonSerializer.Serialize(request);
    //        var content = new StringContent(json, Encoding.UTF8, "application/json");
    //        var response = await _http.PostAsync("api/auth/reset-password", content);
    //        var body = await response.Content.ReadAsStringAsync();
    //        //_logger.LogInformation("Reset API Response: {Body}", body);
    //        var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
    //        return result?.Success ?? false;
    //        //return response.IsSuccessStatusCode;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "ResetPasswordAsync failed for {Email}", email);
    //        return false;
    //    }
    //}
    //public async Task<UserDto?> GetUserByEmailAsync_Internal(string email)
    //{
    //    try
    //    {
    //        // Reuse existing get-user-by-id flow — but we only have email
    //        // So call the API auth check endpoint to see if user exists
    //        // Simplest: call login with a dummy password just to get user info — NO
    //        // Instead call GET api/users filtered by admin, but we have no token here
    //        // Cleanest: add a lightweight check endpoint, OR just attempt send and let it silently fail
    //        // We return a mock UserDto with email so email is sent regardless — actual SP validates
    //        return new UserDto { FullName = email.Split('@')[0], Email = email, IsActive = true };
    //    }
    //    catch { return null; }
    //}

    // Replace old ResetPasswordAsync with:
    public async Task<string?> GenerateResetTokenAsync(string email)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new { Email = email });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/auth/forgot-password", content);
            var body = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Forgot-password API returned {StatusCode}. Body: {Body}", response.StatusCode, body);

            // Token is inside Data.Token if email was found
            //using var doc = JsonDocument.Parse(body);
            //var tokenValue = doc.RootElement
            //                    .GetProperty("data")
            //                    .GetProperty("token")
            //                    .GetString();
            //return tokenValue;
            // Try to deserialize into ApiResponse<JsonElement> so we can inspect Data robustly
            try
            {
                var apiResp = JsonSerializer.Deserialize<ApiResponse<JsonElement>>(body, _jsonOpts);
                if (apiResp?.Data.ValueKind == JsonValueKind.Object)
                {
                    // case-insensitive search for "token" inside Data
                    foreach (var prop in apiResp.Data.EnumerateObject())
                    {
                        if (string.Equals(prop.Name, "token", StringComparison.OrdinalIgnoreCase) &&
                            prop.Value.ValueKind == JsonValueKind.String)
                        {
                            var tokenValue = prop.Value.GetString();
                            if (!string.IsNullOrWhiteSpace(tokenValue))
                                return tokenValue;
                        }
                    }
                }

                _logger.LogWarning("Forgot-password API response did not contain a token for {Email}.", email);
                return null;
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Failed to parse forgot-password API response JSON for {Email}.", email);
                return null;
            }



        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GenerateResetTokenAsync failed.");
            return null;
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var request = new ResetPasswordRequest { Token = token, NewPassword = newPassword };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("api/auth/reset-password", content);
            var body = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(body, _jsonOpts);
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResetPasswordAsync failed.");
            return false;
        }
    }

    public async Task<bool> ValidateResetTokenAsync(Guid token)
    {
        try
        {
            var response = await _http.GetAsync(
                $"api/auth/validate-reset-token?token={token}");
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("ValidateResetToken response: {Body}", body);

            //using var doc = JsonDocument.Parse(body);
            //var isValid = doc.RootElement
            //                   .GetProperty("data")
            //                   .GetProperty("isValid")
            //                   .GetBoolean();
            //return isValid;
            // Use existing _jsonOpts (PropertyNameCaseInsensitive = true)
            //var result = JsonSerializer.Deserialize<ApiResponse<ValidateTokenData>>(body, _jsonOpts);
            //return result?.Data?.IsValid ?? false;
            // Parse manually — avoids any wrapper mismatch
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Check top-level Success first
            if (root.TryGetProperty("Success", out var successProp) ||
                root.TryGetProperty("success", out successProp))
            {
                if (!successProp.GetBoolean()) return false;
            }

            // Navigate into Data — try both casings
            JsonElement dataEl;
            if (!root.TryGetProperty("Data", out dataEl) &&
                !root.TryGetProperty("data", out dataEl))
                return false;

            // Try IsValid — both casings
            if (dataEl.TryGetProperty("IsValid", out var isValidProp) ||
                dataEl.TryGetProperty("isValid", out isValidProp))
            {
                var result = isValidProp.GetBoolean();
                _logger.LogInformation("ValidateResetToken IsValid={Result}", result);
                return result;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateResetTokenAsync failed for token {Token}", token);
            return false;
        }
    }
    // Small inner class — add at bottom of ApiAuthService.cs
    private sealed class ValidateTokenData
    {
        public bool IsValid { get; set; }
    }
}