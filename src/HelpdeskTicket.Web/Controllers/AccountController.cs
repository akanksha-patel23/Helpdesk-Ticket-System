using HelpdeskTicket.Application.DTOs.Auth;
using HelpdeskTicket.Core.Constants;
using HelpdeskTicket.Web.Helpers;
using HelpdeskTicket.Web.Services;
using HelpdeskTicket.Web.ViewModels.Auth;
using HelpdeskTicket.Application.Services;
using HelpdeskTicket.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.Web.Controllers;

// TODO: Implement in step-by-step coding phase
public class AccountController : Controller {
    private readonly ApiAuthService _authService;
    private readonly ApiUserService _userService;
    private readonly IEmailService _emailService;
    //private readonly UsedTokenStore _usedTokens;
    //private readonly IDataProtectionProvider _protectionProvider;
    private readonly ILogger<AccountController> _logger;

    public AccountController(ApiAuthService authService, ApiUserService userService,
    IEmailService emailService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _userService = userService;
        _emailService = emailService;
        //_protectionProvider = protectionProvider;
        //_usedTokens = usedTokenStore; //remove later, was flag based but now using single-use tokens in data protection
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToDashboard(SessionHelper.GetRoleId(HttpContext.Session));

        return View(new LoginViewModel());
    }

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Login(LoginViewModel model)
    //{
    //    if (!ModelState.IsValid)
    //        return View(model);

    //    var request = new LoginRequest { Email = model.Email, Password = model.Password, RememberMe = model.RememberMe };
    //    var response = await _authService.LoginAsync(request);

    //    if (response is null || !response.Success || response.Data is null)
    //    {
    //        ModelState.AddModelError(string.Empty, response?.Message ?? "Login failed. Please try again.");
    //        return View(model);
    //    }

    //    var data = response.Data;

    //    SessionHelper.SetUserSession(
    //        HttpContext.Session,
    //        data.UserId, data.FullName, data.Email,
    //        data.RoleId, data.RoleName, data.Token);

    //    _logger.LogInformation("User {Email} logged in as {Role}", data.Email, data.RoleName);
    //    return RedirectToDashboard(data.RoleId);
    //}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var request = new LoginRequest { Email = model.Email, Password = model.Password, RememberMe = model.RememberMe };
        var response = await _authService.LoginAsync(request);

        if (response is null || !response.Success || response.Data is null)
        {
            ModelState.AddModelError(string.Empty, response?.Message ?? "Login failed. Please try again.");
            return View(model);
        }

        var data = response.Data;

        SessionHelper.SetUserSession(
            HttpContext.Session,
            data.UserId, data.FullName, data.Email,
            data.RoleId, data.RoleName, data.Token);

        // Remember Me — write a persistent cookie with the token
        // expires in 30 days
        if (model.RememberMe)
        {
            Response.Cookies.Append("hd_remember", data.Token, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
        }

        _logger.LogInformation("User {Email} logged in. RememberMe:{R}", data.Email, model.RememberMe);
        return RedirectToDashboard(data.RoleId);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToDashboard(SessionHelper.GetRoleId(HttpContext.Session));

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var request = new RegisterRequest
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password,
            //RoleId = model.RoleId
            RoleId = Roles.EndUserId //always enduser
        };

        var response = await _authService.RegisterAsync(request);

        if (response is null || !response.Success)
        {
            ModelState.AddModelError(string.Empty, response?.Message ?? "Registration failed. Please try again.");
            return View(model);
        }
        // Attempt to auto-login the newly registered user so we can go straight to the dashboard.
        var loginResponse = await _authService.LoginAsync(new LoginRequest
        {
            Email = model.Email,
            Password = model.Password,
            RememberMe = false
        });

        if (loginResponse != null && loginResponse.Success && loginResponse.Data is not null)
        {
            var data = loginResponse.Data;
            SessionHelper.SetUserSession(
                HttpContext.Session,
                data.UserId, data.FullName, data.Email,
                data.RoleId, data.RoleName, data.Token);

            // Mark as new user so the Dashboard can show "Welcome" instead of "Welcome back"
            TempData["IsNewUser"] = true;

            _logger.LogInformation("User {Email} registered and auto-logged in as {Role}.", model.Email, data.RoleName);
            return RedirectToDashboard(data.RoleId);
        }

        // Fallback: if auto-login failed, send user to the login page as before
        TempData["SuccessMessage"] = "Registration successful! Please log in.";
        return RedirectToAction(nameof(Login));
    }
    //TempData["SuccessMessage"] = "Registration successful! Please log in.";
    //    return RedirectToAction(nameof(Login));
    //}

    //logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        _logger.LogInformation("User {Email} logged out.", SessionHelper.GetEmail(HttpContext.Session));
        SessionHelper.ClearSession(HttpContext.Session);
        Response.Cookies.Delete("hd_remember"); // clear persistent cookie on logout
        return RedirectToAction(nameof(Login));
    }
    //role based redirection
    private IActionResult RedirectToDashboard(int roleId) => roleId switch
    {
        Roles.AdminId => RedirectToAction("Index", "Dashboard"),
        Roles.DeveloperId => RedirectToAction("Index", "Dashboard"),
        _ => RedirectToAction("Index", "Dashboard")
    };

    // Inject these in constructor — add to existing constructor params:
    // IDataProtectionProvider protectionProvider,
    // EmailService emailService,
    // ApiUserService userService

    // ?? Forgot Password GET 
    [HttpGet]
    public IActionResult ForgotPassword()
        => View(new ForgotPasswordViewModel());

    // ?? Forgot Password POST 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Look up user via existing user API
        //var user = await _userService.GetUserByEmailAsync_Internal(model.Email);
        //var user = await _authService.GetUserByEmailAsync_Internal(model.Email);

        //if (user is not null && user.IsActive)
        //{
        //    // Generate tamper-proof token — expires in 30 minutes
        //    var protector = _protectionProvider.CreateProtector("PasswordReset");
        //    var payload = $"{model.Email}|{DateTime.UtcNow.AddMinutes(30):O}";
        //    var token = protector.Protect(payload);

        //    var resetLink = Url.Action("ResetPassword", "Account",
        //        new { token = Uri.EscapeDataString(token) },
        //        protocol: Request.Scheme)!;

        //    await _emailService.SendResetLinkAsync(model.Email, user.FullName, resetLink);
        //}

        // Generate token in DB — returns GUID if email found, null if not
        var token = await _authService.GenerateResetTokenAsync(model.Email);
        if (token is null)
        {
            // Email not found in DB — show specific error
            ModelState.AddModelError(nameof(model.Email), "No account found with this email address.");
            return View(model);
        }

        if (token is not null)
        {
            var resetLink = Url.Action("ResetPassword", "Account", new { token }, protocol: Request.Scheme)!;
            _logger.LogInformation("Generated reset token for {Email}. Token: {Token}. ResetLink: {ResetLink}", model.Email, token, resetLink);

            var displayName = model.Email.Split('@')[0];
            //await _emailService.SendResetLinkAsync(model.Email, displayName, resetLink);
            try
            {
                await _emailService.SendResetLinkAsync(model.Email, displayName, resetLink);
                _logger.LogInformation("Reset link email queued/sent to {Email}", model.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reset link to {Email}", model.Email);
            }
        }
        else
        {
            _logger.LogInformation("GenerateResetTokenAsync returned null for {Email} (silent by design).", model.Email);
        }

        TempData["SuccessMessage"] =
            "Password reset link has been sent to your email.";
        return RedirectToAction(nameof(ForgotPassword));
    }

    // ?? Reset Password GET 
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return RedirectToAction(nameof(Login));
        // Validate token exists and is not yet used — check DB before showing form
        if (!Guid.TryParse(token, out var tokenGuid))
        {
            TempData["ErrorMessage"] = "Invalid reset link.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        var isValid = await _authService.ValidateResetTokenAsync(tokenGuid);

        if (!isValid)
        {
            TempData["ErrorMessage"] =
                "This reset link has already been used or has expired. Please request a new one.";
            return RedirectToAction(nameof(ForgotPassword));
        }
        return View(new ResetPasswordViewModel { Token = token });
    }

    // ?? Reset Password POST 
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        //try
        //{
        //    var protector = _protectionProvider.CreateProtector("PasswordReset");
        //    var payload = protector.Unprotect(Uri.UnescapeDataString(model.Token));
        //    var parts = payload.Split('|');

        //    if (parts.Length != 2 || DateTime.Parse(parts[1]) < DateTime.UtcNow)
        //    {
        //        ModelState.AddModelError(string.Empty,
        //            "This reset link has expired. Please request a new one.");
        //        return View(model);
        //    }

        //    // ?? Single-use check ??????????????????????????????????
        //    // TryUse returns false if token was already consumed
        //    //if (!_usedTokens.TryUse(model.Token))
        //    //{
        //    //    ModelState.AddModelError(string.Empty,
        //    //        "This reset link has already been used. Please request a new one.");
        //    //    return View(model);
        //    //}

        //    var email = parts[0];
        //    //var response = await _authApiService.ResetPasswordAsync(email, model.NewPassword);
        //    var response = await _authService.ResetPasswordAsync(email, model.NewPassword);

        //    if (!response)
        //    {
        //        ModelState.AddModelError(string.Empty, "Password reset failed. Please try again.");
        //        return View(model);
        //    }

        // Token is GUID from query string — no decryption needed
        var success = await _authService.ResetPasswordAsync(model.Token, model.NewPassword);

        if (!success)
        {
            ModelState.AddModelError(string.Empty,
                "Invalid, expired, or already used reset link. Please request a new one.");
            return View(model);
        }

        // Ensure the user is signed out so /Account/Login doesn't immediately redirect
        // to the dashboard when Session still contains an auth token or persistent cookie.
        try
        {
            SessionHelper.ClearSession(HttpContext.Session);
            Response.Cookies.Delete("hd_remember");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear session/cookie after password reset.");
        }

        TempData["SuccessMessage"] = "Password reset successfully. Please log in.";
        return RedirectToAction(nameof(Login));

        //catch
        //{
        //    ModelState.AddModelError(string.Empty,
        //        "Invalid or expired reset link. Please request a new one.");
        //    return View(model);
        //}
    }
}
