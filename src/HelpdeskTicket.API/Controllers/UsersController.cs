using HelpdeskTicket.Application.DTOs.User;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.API.Controllers;

// TODO: Implement in step-by-step coding phase
[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase 
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    // GET api/users/developers — active developers for assignment dropdown
    [HttpGet("developers")]
    public async Task<IActionResult> GetDevelopers()
    {
        var developers = await _userService.GetDevelopersAsync();
        return Ok(ApiResponse<List<UserDto>>.Ok(developers));
    }
    // GET api/users?requestedRoleId=1&requestedById=1
    [HttpGet]
    public async Task<IActionResult> GetUserList(
        [FromQuery] int requestedRoleId,
        [FromQuery] int requestedById)
    {
        if (requestedRoleId <= 0 || requestedById <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid parameters."));

        var users = await _userService.GetUserListAsync(requestedRoleId, requestedById);
        return Ok(ApiResponse<List<UserDto>>.Ok(users));
    }

    // GET api/users/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<object>.Fail("User not found."));

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    // POST api/users  — Admin creates user with any role
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var (success, message) = await _userService.CreateUserAsync(request);
        if (!success) return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { }, message));
    }

    // PUT api/users/{id}  — Admin updates role / active status
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.UserId)
            return BadRequest(ApiResponse<object>.Fail("User ID mismatch."));

        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var (success, message) = await _userService.UpdateUserAsync(request);
        if (!success) return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { }, message));
    }
    
}
