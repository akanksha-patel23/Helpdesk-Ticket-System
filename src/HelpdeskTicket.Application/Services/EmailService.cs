using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Drawing;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace HelpdeskTicket.Application.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
    //private const string BaseUrl = "https://localhost:7000"; // TODO: move to settings if needed for ticket links
    //private const string BaseUrl = _settings.BaseUrl;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", toEmail, subject);
            // Swallow — email failure should never break the ticket operation
        }
    }

    private static string Wrap(string headerColor, string heading, string bodyHtml) => $"""
        <div style="font-family:Arial,sans-serif;max-width:560px;margin:40px auto;
                    border:1px solid #e2e8f0;border-radius:12px;overflow:hidden;">
            <div style="background:{headerColor};padding:24px 32px;">
                <h2 style="color:#fff;margin:0;font-size:1.2rem;">🎧 Helpdesk System</h2>
            </div>
            <div style="padding:32px;">
                <h3 style="margin-top:0;">{heading}</h3>
                {bodyHtml}
            </div>
        </div>
        """;

    public async Task SendResetLinkAsync(string toEmail, string toName, string resetLink)
    {
        var body = $"""
            <p>Hi <strong>{toName}</strong>,</p>
            <p>We received a request to reset your password. Click below to set a new password.</p>
            <a href="{resetLink}" style="display:inline-block;margin:16px 0;padding:12px 28px;
               background:#3b82f6;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;">
                Reset Password
            </a>
            <p style="color:#64748b;font-size:.85rem;">
                This link expires in <strong>30 minutes</strong>.<br/>
                If you did not request this, you can safely ignore this email.
            </p>
            """;
        await SendAsync(toEmail, toName, "Helpdesk — Password Reset Request", Wrap("#1e293b", "Password Reset Request", body));
    }

    public async Task SendTicketCreatedAsync(string toEmail, string toName, int ticketId, string title, string description)
    {
        var snippet = description.Length > 150 ? description[..150] + "…" : description;
        //var link = $"{BaseUrl}/Ticket/Details/{ticketId}";
        var link = $"{_settings.BaseUrl}/Ticket/Details/{ticketId}";
        var body = $"""
            <p>Hi <strong>{toName}</strong>,</p>
            <p>A new ticket has been raised and requires action.</p>
            <!-- <p><strong>Ticket #{ticketId}:</strong> {title}</p> -->
            <p><strong>Ticket's Title: </strong> {title}</p>
            <p style="margin-bottom: 4px;">Description of Ticket: </p>
            <p style="color:#64748b;">{snippet}</p>
            <a href="{link}" style="display:inline-block;margin:16px 0;padding:12px 28px;
               background:#3b82f6;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;">
                View Ticket
            </a>
            """;
        //await SendAsync(toEmail, toName, $"New Ticket Raised — #{ticketId}", Wrap("#1e293b", "New Ticket Raised", body));
        await SendAsync(toEmail, toName, $"New Ticket Raised", Wrap("#1e293b", "New Ticket Raised", body));
    }

    public async Task SendTicketAssignedAsync(string toEmail, string toName, int ticketId, string title, string description, string assignedToName, bool isOwner)
    {
        var snippet = description.Length > 150 ? description[..150] + "…" : description;
        //var link = $"{BaseUrl}/Ticket/Details/{ticketId}";
        var link = $"{_settings.BaseUrl}/Ticket/Details/{ticketId}";
        var body = $"""
            <p>Hi <strong>{toName}</strong>,</p>
            <!-- <p>Ticket <strong>#{ticketId}: {title}</strong> has been assigned to <strong>{assignedToName}</strong>.</p> -->
            <!-- <p>{(isOwner ? "Your" : "The")} Ticket titled: "<strong>{title}</strong>" has been assigned to <strong>{assignedToName}</strong>.</p> -->
            <p>{(isOwner ? "Your" : "The")} Ticket titled: "<strong>{title}</strong>" has been assigned to <strong>{(isOwner ? assignedToName : "you")}</strong>.</p>
            <p style="margin-bottom: 4px;">Description of Ticket: </p>
            <p style="color:#64748b;">{snippet}</p>
            <a href="{link}" style="display:inline-block;margin:16px 0;padding:12px 28px;
               background:#3b82f6;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;">
                View Ticket
            </a>
            """;
        //await SendAsync(toEmail, toName, $"Ticket Assigned — #{ticketId}", Wrap("#0891b2", "Ticket Assigned", body));
        await SendAsync(toEmail, toName, $"Ticket Assigned", Wrap("#0891b2", "Ticket Assigned", body));
    }

    public async Task SendStatusChangedAsync(string toEmail, string toName, int ticketId, string title, string description, string newStatus, bool richContent, bool isOwner)
    {
        //var link = $"{BaseUrl}/Ticket/Details/{ticketId}";
        var link = $"{_settings.BaseUrl}/Ticket/Details/{ticketId}";

        string body;
        if (richContent)
        {
            var snippet = description.Length > 150 ? description[..150] + "…" : description;
            body = $"""
                <p>Hi <strong>{toName}</strong>,</p>
                <!-- <p>Ticket <strong>#{ticketId}: {title}</strong> status changed to <strong>{newStatus}</strong>.</p> -->
                <p>{(isOwner ? "Your" : "The")} Ticket's status titled: "<strong>{title}</strong>" has been changed to <strong>{newStatus}</strong>.</p>
                <p style="margin-bottom: 4px;">Description of Ticket: </p>
                <p style="color:#64748b;">{snippet}</p>
                <a href="{link}" style="display:inline-block;margin:16px 0;padding:12px 28px;
                   background:#3b82f6;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;">
                    View Ticket
                </a>
                """;
        }
        else
        {
            //body = $"""
            //    <p>Hi <strong>{toName}</strong>,</p>
            //    <p>Ticket <strong>#{ticketId}: {title}</strong> is now <strong>{newStatus}</strong>. {description}</p>
            //    <a href="{link}" style="color:#3b82f6;">View Ticket</a>
            //    """;
            var liteSnippet = description.Length > 100 ? description[..100] + "…" : description;
            body = $"""
            <p>Hi <strong>{toName}</strong>,</p>
            <p>{(isOwner ? "Your" : "The")} Ticket titled <strong>{title}</strong> is now <strong>{newStatus}</strong>.</p>
            <p style="margin-bottom: 4px;">Description of Ticket: </p>
            <p style="color:#64748b;">{liteSnippet}</p>
            <a href="{link}" style="display:inline-block;margin:16px 0;padding:12px 28px;
               background:#3b82f6;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;">
                View Ticket
            </a>
            """;
        }

        //await SendAsync(toEmail, toName, $"Ticket #{ticketId} — {newStatus}", Wrap("#f59e0b", $"Ticket {newStatus}", body));
        var color = newStatus switch
        {
            "In Progress" => "#f59e0b",
            "Resolved" => "#16a34a",
            _ => "#f59e0b"
        };
        await SendAsync(toEmail, toName, $"Ticket — {newStatus}", Wrap(color, $"Ticket {newStatus}", body));
    }

    public async Task SendTicketClosedAsync(string toEmail, string toName, int ticketId, string title, string description, bool isOwner)
    {
        var snippet = description.Length > 150 ? description[..150] + "…" : description;
        //var link = $"{BaseUrl}/Ticket/Details/{ticketId}";
        var link = $"{_settings.BaseUrl}/Ticket/Details/{ticketId}";
        var body = $"""
            <p>Hi <strong>{toName}</strong>,</p>
            <!-- <p>Ticket <strong>#{ticketId}: {title}</strong> has been closed.</p> -->
            <p>{(isOwner ? "Your" : "The")} Ticket titled: "<strong>{title}</strong>" has been closed.</p>
            <p style="margin-bottom: 4px;">Description of Ticket: </p>
            <p style="color:#64748b;">{snippet}</p>
            <a href="{link}" style="display:inline-block;margin:16px 0;padding:12px 28px;
               background:#3b82f6;color:#fff;border-radius:8px;text-decoration:none;font-weight:600;">
                View Ticket
            </a>
            """;
        //await SendAsync(toEmail, toName, $"Ticket Closed — #{ticketId}", Wrap("#1e293b", "Ticket Closed", body));
        await SendAsync(toEmail, toName, $"Ticket Closed", Wrap("#1e293b", "Ticket Closed", body));
    }
}
