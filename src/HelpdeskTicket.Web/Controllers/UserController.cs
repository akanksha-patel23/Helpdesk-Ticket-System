using HelpdeskTicket.Application.DTOs.User;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Constants;
using HelpdeskTicket.Web.Helpers;
using HelpdeskTicket.Web.Services;
using HelpdeskTicket.Web.ViewModels.User;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.Web.Controllers;

// TODO: Implement in step-by-step coding phase
public class UserController : Controller {
    private readonly ApiUserService _userService;
    private readonly ApiAuthService _apiAuthService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UserController> _logger;

    public UserController(ApiUserService userService, ApiAuthService apiAuthService, IEmailService emailService, ILogger<UserController> logger)
    {
        _userService = userService;
        _apiAuthService = apiAuthService;
        _emailService = emailService;
        _logger = logger;
    }

    // ── ADMIN: User list ──────────────────────────────────────
    // GET /User/Index
    public async Task<IActionResult> Index(string? searchText = null)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId)
            return RedirectToAction("Index", "Dashboard");

        var users = await _userService.GetUserListAsync();
        var model = new UserListViewModel { Users = users, SearchText = searchText };
        return View(model);
    }

    // ── ADMIN: Create user form GET ───────────────────────────
    [HttpGet]
    public IActionResult Create()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session)) return RedirectToAction("Login", "Account");
        if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId) return RedirectToAction("Index", "Dashboard");

        return View(new UserFormViewModel());
    }

    // ── ADMIN: Create user POST ───────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session)) return RedirectToAction("Login", "Account");
        if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId) return RedirectToAction("Index", "Dashboard");

        // Password required on create
        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError("Password", "Password is required when creating a user.");

        if (!ModelState.IsValid)
            return View(model);

        var request = new CreateUserRequest
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password!,
            RoleId = model.RoleId
        };

        var (success, message) = await _userService.CreateUserAsync(request);
        if (!success) { ModelState.AddModelError(string.Empty, message); return View(model); }

        TempData["SuccessMessage"] = message;
        return RedirectToAction(nameof(Index));
    }

    // ── ADMIN: Edit user form GET ─────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session)) return RedirectToAction("Login", "Account");
        if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId) return RedirectToAction("Index", "Dashboard");

        var user = await _userService.GetUserByIdAsync(id);
        if (user is null) { TempData["ErrorMessage"] = "User not found."; return RedirectToAction(nameof(Index)); }

        return View(UserFormViewModel.FromDto(user));
    }

    // ── ADMIN: Edit user POST ─────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserFormViewModel model)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session)) return RedirectToAction("Login", "Account");
        if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId) return RedirectToAction("Index", "Dashboard");

        // Remove Password validation — not required on edit
        ModelState.Remove("Password");

        if (!ModelState.IsValid)
            return View(model);

        var request = new UpdateUserRequest
        {
            UserId = model.UserId,
            FullName = model.FullName,
            Email = model.Email,
            RoleId = model.RoleId,
            IsActive = model.IsActive
        };

        var (success, message) = await _userService.UpdateUserAsync(request);
        if (!success) { ModelState.AddModelError(string.Empty, message); return View(model); }

        TempData["SuccessMessage"] = message;
        return RedirectToAction(nameof(Index));
    }

    // ── ADMIN: Send password reset link to user ──────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendResetLink(int userId)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId)
            return RedirectToAction("Index", "Dashboard");

        var user = await _userService.GetUserByIdAsync(userId);
        if (user is null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!user.IsActive)
        {
            TempData["ErrorMessage"] = "Cannot send reset link to an inactive user.";
            return RedirectToAction(nameof(Index));
        }

        var token = await _apiAuthService.GenerateResetTokenAsync(user.Email);
        if (token is null)
        {
            TempData["ErrorMessage"] = "Failed to generate reset token for the user.";
            return RedirectToAction(nameof(Index));
        }

        var resetLink = Url.Action("ResetPassword", "Account", new { token }, protocol: Request.Scheme)!;
        var displayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email.Split('@')[0] : user.FullName;

        try
        {
            await _emailService.SendResetLinkAsync(user.Email, displayName, resetLink);
            TempData["SuccessMessage"] = "Password reset link sent to user's email.";
            _logger.LogInformation("Admin sent reset link to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reset email to {Email}", user.Email);
            TempData["ErrorMessage"] = "Failed to send email. Check logs for details.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ── ALL ROLES: Profile page ───────────────────────────────
    // GET /User/Profile
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        var userId = SessionHelper.GetUserId(HttpContext.Session);
        var user = await _userService.GetUserByIdAsync(userId);

        if (user is null)
        {
            TempData["ErrorMessage"] = "Could not load profile.";
            return RedirectToAction("Index", "Dashboard");
        }

        var model = new ProfileViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.RoleName,
            CreatedDateTime = user.CreatedDateTime
        };

        return View(model);
    }
    // ── ALL ROLES: Profile POST — update own name + email ─────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
            return View(model);

        var session = HttpContext.Session;

        // Fetch current user to preserve RoleId and IsActive
        // — user cannot change their own role or status
        var current = await _userService.GetUserByIdAsync(model.UserId);
        if (current is null)
        {
            TempData["ErrorMessage"] = "Could not update profile.";
            return RedirectToAction("Index", "Dashboard");
        }

        var request = new UpdateUserRequest
        {
            UserId = model.UserId,
            FullName = model.FullName,
            Email = model.Email,
            RoleId = current.RoleId,    // locked — not from form
            IsActive = current.IsActive   // locked — not from form
        };

        var (success, message) = await _userService.UpdateUserAsync(request);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            model.RoleName = current.RoleName;
            model.CreatedDateTime = current.CreatedDateTime;
            return View(model);
        }

        // Update session with new name so sidebar reflects immediately
        SessionHelper.SetUserSession(
            session,
            current.Id, model.FullName, model.Email,
            current.RoleId, current.RoleName,
            SessionHelper.GetToken(session)!);

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }
}
