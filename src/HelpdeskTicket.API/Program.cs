using System.Text;
using HelpdeskTicket.API.Extensions;
using HelpdeskTicket.API.Middleware;
using HelpdeskTicket.Core.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

//  HelpdeskTicket.API — Program.cs

var builder = WebApplication.CreateBuilder(args);

// ── 1. Strongly-typed configuration ──────────────────────────
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.Configure<FileUploadSettings>(
    builder.Configuration.GetSection(FileUploadSettings.SectionName));

builder.Services.Configure<PaginationSettings>(
    builder.Configuration.GetSection(PaginationSettings.SectionName));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

// ── 2. Controllers ────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null); // keep PascalCase

// ── 3. JWT Authentication ─────────────────────────────────────
var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings missing from appsettings.json.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken            = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings.Issuer,
        ValidateAudience         = true,
        ValidAudience            = jwtSettings.Audience,
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ── 4. Application services (Repository + Service registrations)
//       Added here module-by-module as coding progresses.
builder.Services.AddApplicationServices(builder.Configuration);

// ── 5. Swagger ────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Helpdesk Ticket System API",
        Version     = "v1",
        Description = "All DB operations via SQL Server Stored Procedures (ADO.NET)."
    });

    // JWT Bearer auth button in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── 6. Logging ────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ── 7. CORS — allow Web MVC project to call API ───────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("HelpdeskWebPolicy", policy =>
        policy.WithOrigins("http://localhost:7000", "https://localhost:7000", "http://localhost:5000")
        //policy.WithOrigins("http://www.helpdesk.com")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials());

});

// ── 8. HttpContextAccessor ────────────────────────────────────
builder.Services.AddHttpContextAccessor();

//  BUILD + PIPELINE
var app = builder.Build();

// 1. Global exception handler — always first
app.UseMiddleware<GlobalExceptionMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(o =>
//    {
//        o.SwaggerEndpoint("/swagger/v1/swagger.json", "Helpdesk Ticket System API v1");
//        o.RoutePrefix = "swagger";
//    });
//}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Helpdesk API v1");
});

//app.UseHttpsRedirection();
app.UseCors("HelpdeskWebPolicy");

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
