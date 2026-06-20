using HelpdeskTicket.Core.Settings;
using HelpdeskTicket.Web.Helpers;
using HelpdeskTicket.Web.Middleware;
using HelpdeskTicket.Web.Services;
using Microsoft.AspNetCore.CookiePolicy;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Application.Services;

// communicates with API via named HttpClient.
// No direct DB access. JWT stored in server-side Session.

var builder = WebApplication.CreateBuilder(args);

// ── 1. Strongly-typed configuration ──────────────────────────
builder.Services.Configure<SessionSettings>(
    builder.Configuration.GetSection(SessionSettings.SectionName));

builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection(FileUploadSettings.SectionName));

builder.Services.Configure<PaginationSettings>(
    builder.Configuration.GetSection(PaginationSettings.SectionName));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

// ── 2. MVC + Razor ────────────────────────────────────────────
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();   // hot-reload views without rebuild in dev

// ── 3. Session ────────────────────────────────────────────────
var sessionSettings = builder.Configuration
    .GetSection(SessionSettings.SectionName)
    .Get<SessionSettings>() ?? new SessionSettings();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(sessionSettings.IdleTimeoutMinutes);
    options.Cookie.Name        = sessionSettings.CookieName;
    options.Cookie.HttpOnly    = sessionSettings.HttpOnly;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = sessionSettings.SecurePolicy
        .Equals("Always", StringComparison.OrdinalIgnoreCase)
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
});

// ── 4. Named HttpClient — calls HelpdeskTicket.API ────────────
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl missing from appsettings.json.");

builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient("HelpdeskAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout     = TimeSpan.FromSeconds(
        builder.Configuration.GetValue<int>("ApiSettings:TimeoutSeconds", 30));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
   .AddHttpMessageHandler<AuthHeaderHandler>();   // ← attaches JWT to every request

// ── 5. HttpContextAccessor ────────────────────────────────────
// Required by Web Services to read session (JWT, user info)
// when building HttpClient requests.
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();  // enables IDataProtector for token generation


// ── 6. Web Services ───────────────────────────────────────────
// Registered here module-by-module as coding progresses.
builder.Services.AddScoped<ApiAuthService>();
builder.Services.AddScoped<ApiDashboardService>();
builder.Services.AddScoped<ApiTicketService>();
builder.Services.AddScoped<ApiUserService>();
builder.Services.AddScoped<ApiCommentService>();
builder.Services.AddScoped<ApiCategoryService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<UsedTokenStore>();

// ── 7. Logging ────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


//  BUILD + PIPELINE
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();       // serves wwwroot (css, js, images, uploads)

app.UseRouting();

// Session MUST be after UseRouting and before UseAuthorization
app.UseSession();
app.UseMiddleware<RememberMeMiddleware>();
app.UseAuthorization();

// Default MVC route
app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}");
pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
