using HelpdeskTicket.Application.DTOs.Auth;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HelpdeskTicket.Application.Services;

// TODO: Implement in step-by-step coding phase
public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly IPasswordHasher<string> _hasher;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAuthRepository authRepo,
        IPasswordHasher<string> hasher,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthService> logger)
    {
        _authRepo = authRepo;
        _hasher = hasher;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _authRepo.GetUserByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
            return null;

        var result = _hasher.VerifyHashedPassword(user.Email, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        var token = GenerateJwtToken(user.Id, user.Email, user.FullName, user.RoleId, user.RoleName);

        return new LoginResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.RoleName,
            Token = token
        };
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        var existing = await _authRepo.GetUserByEmailAsync(request.Email);
        if (existing is not null)
            return (false, "This email is already registered.");

        var hash = _hasher.HashPassword(request.Email, request.Password);
        var newId = await _authRepo.RegisterUserAsync(request.FullName, request.Email, hash, request.RoleId);

        if (newId <= 0)
            return (false, "Registration failed. Please try again.");

        _logger.LogInformation("User registered: {Email} RoleId: {RoleId}", request.Email, request.RoleId);
        return (true, "Registration successful.");
    }

    private string GenerateJwtToken(int userId, string email, string fullName, int roleId, string roleName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name,               fullName),
            new Claim(ClaimTypes.Role,               roleName),
            new Claim("userId",                      userId.ToString()),
            new Claim("roleId",                      roleId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    //public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
    //{
    //    var user = await _authRepo.GetUserByEmailAsync(request.Email);
    //    if (user is null || !user.IsActive)
    //        return (false, "No active account found with this email.");

    //    var hash = _hasher.HashPassword(request.Email, request.NewPassword);
    //    var updated = await _authRepo.UpdatePasswordAsync(request.Email, hash);
    //    _logger.LogInformation("Updated = {Updated}", updated);

    //    // TEMP TEST
    //    //updated = true;
    //    if (!updated)
    //        return (false, "Password reset failed. Please try again.");

    //    _logger.LogInformation("Password reset for {Email}", request.Email);
    //    return (true, "Password reset successfully.");
    //}

    // Remove old ResetPasswordAsync — replace with these two:

    public async Task<(bool Success, string Message)> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _authRepo.GetUserByEmailAsync(email);
        if (user is null || !user.IsActive)
            return (false, "No active account found.");

        var token = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddMinutes(30);

        var saved = await _authRepo.SavePasswordResetTokenAsync(email, token, expiry);
        if (!saved)
            return (false, "Could not generate reset token.");

        _logger.LogInformation("Reset token generated for {Email}", email);
        return (true, token.ToString());   // token string returned to caller
    }

    public async Task<(bool Success, string Message)> ResetPasswordByTokenAsync(
        Guid token, string newPassword)
    {
        var hash = _hasher.HashPassword(string.Empty, newPassword);
        return await _authRepo.ResetPasswordByTokenAsync(token, hash);
    }
    public async Task<bool> ValidateResetTokenAsync(Guid token)
    => await _authRepo.ValidateResetTokenAsync(token);
}
