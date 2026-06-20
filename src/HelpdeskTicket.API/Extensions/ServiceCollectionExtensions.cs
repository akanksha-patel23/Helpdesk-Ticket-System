using HelpdeskTicket.Application.Data;
using Microsoft.AspNetCore.Identity;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Application.Repositories;
using HelpdeskTicket.Application.Services;

namespace HelpdeskTicket.API.Extensions;

// ─────────────────────────────────────────────────────────────
//  ServiceCollectionExtensions (API)
//
//  Central DI registration for all Application-layer services.
//  Program.cs calls AddApplicationServices() — stays clean.
//  Module registrations are uncommented one-by-one as built.
// ─────────────────────────────────────────────────────────────
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Database connection factory ───────────────────────
        
        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        // ── ASP.NET Core PasswordHasher ───────────────────────
        // Singleton: stateless, safe to share across requests.
        services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();

        // ── Auth ──────────────────────────────────────────────
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthService, AuthService>();

        // Dashboard
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IDashboardService, DashboardService>();

        // ── Ticket ────────────────────────────────────────────
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketService, TicketService>();

        // ── Comment ───────────────────────────────────────────
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICommentService, CommentService>();

        // ── User ──────────────────────────────────────────────
         services.AddScoped<IUserRepository, UserRepository>();
         services.AddScoped<IUserService, UserService>();

        // ── Category ──────────────────────────────────────────
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();

        // ── Email ──────────────────────────────────────────
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
