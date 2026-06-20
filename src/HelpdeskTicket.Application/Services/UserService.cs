using HelpdeskTicket.Application.DTOs.User;
using HelpdeskTicket.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HelpdeskTicket.Application.Services;

// TODO: Implement in step-by-step coding phase
public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;
    private readonly IAuthRepository _authRepo;
    private readonly IPasswordHasher<string> _hasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepo,
        IAuthRepository authRepo,
        IPasswordHasher<string> hasher,
        ILogger<UserService> logger)
    {
        _userRepo = userRepo;
        _authRepo = authRepo;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetUserListAsync(int requestedRoleId, int requestedById)
    {
        _logger.LogInformation("GetUserList RoleId:{Role} RequestedBy:{Id}", requestedRoleId, requestedById);
        return await _userRepo.GetUserListAsync(requestedRoleId, requestedById);
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        _logger.LogInformation("GetUserById UserId:{Id}", userId);
        return await _userRepo.GetUserByIdAsync(userId);
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(UpdateUserRequest request)
    {
        _logger.LogInformation("UpdateUser UserId:{Id}", request.UserId);
        return await _userRepo.UpdateUserAsync(request);
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request)
    {
        // Reuse AuthRepository.RegisterUserAsync — same SP, same hashing
        var existing = await _authRepo.GetUserByEmailAsync(request.Email);
        if (existing is not null)
            return (false, "This email is already registered.");

        var hash = _hasher.HashPassword(request.Email, request.Password);
        var newId = await _authRepo.RegisterUserAsync(
            request.FullName, request.Email, hash, request.RoleId);

        if (newId <= 0)
            return (false, "Failed to create user. Please try again.");

        _logger.LogInformation("Admin created user: {Email} RoleId:{Role}", request.Email, request.RoleId);
        return (true, "User created successfully.");
    }
    public async Task<List<UserDto>> GetDevelopersAsync()
    {
        _logger.LogInformation("GetDevelopersAsync called.");
        return await _userRepo.GetDevelopersAsync();
    }
}
