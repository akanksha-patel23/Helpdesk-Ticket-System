using HelpdeskTicket.Application.DTOs.Auth;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.API.Controllers;

// TODO: Implement in step-by-step coding phase
[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase {
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request data."));

        var result = await _authService.LoginAsync(request);

        if (result is null)
            return Unauthorized(ApiResponse<object>.Fail("Invalid email or password."));

        _logger.LogInformation("User logged in: {Email}", request.Email);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "Login successful."));
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Invalid request data."));

        var (success, message) = await _authService.RegisterAsync(request);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        _logger.LogInformation("New user registered: {Email}", request.Email);
        return Ok(ApiResponse<object>.Ok(new { }, message));
    }
    // POST api/auth/reset-password
    //[HttpPost("reset-password")]
    //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    //{
    //    if (!ModelState.IsValid)
    //        return BadRequest(ApiResponse<object>.Fail("Invalid request data."));

    //    var (success, message) = await _authService.ResetPasswordAsync(request);

    //    _logger.LogInformation(
    //    "ResetPassword API -> Success:{Success}, Message:{Message}",
    //    success, message);

    //    if (!success)
    //        return BadRequest(ApiResponse<object>.Fail(message));

    //    return Ok(ApiResponse<object>.Ok(new { }, message));
    //}

    // POST api/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(ApiResponse<object>.Fail("Email is required."));

        var (success, tokenOrError) = await _authService.GeneratePasswordResetTokenAsync(request.Email);

        // Always return OK — never reveal if email exists
        return Ok(ApiResponse<object>.Ok(
            new { Token = success ? tokenOrError : null },
            "If that email is registered, a reset link has been sent."));
    }

    // GET api/auth/validate-reset-token?token=...
    [HttpGet("validate-reset-token")]
    public async Task<IActionResult> ValidateResetToken([FromQuery] Guid token)
    {
        var isValid = await _authService.ValidateResetTokenAsync(token);
        return Ok(ApiResponse<object>.Ok(new { IsValid = isValid }));
    }

    // POST api/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        if (!Guid.TryParse(request.Token, out var tokenGuid))
            return BadRequest(ApiResponse<object>.Fail("Invalid reset token."));

        var (success, message) = await _authService.ResetPasswordByTokenAsync(tokenGuid, request.NewPassword);

        if (!success) return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { }, message));
    }
}
