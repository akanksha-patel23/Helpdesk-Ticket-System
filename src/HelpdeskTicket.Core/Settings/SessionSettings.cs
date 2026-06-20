namespace HelpdeskTicket.Core.Settings;

// ─────────────────────────────────────────────────────────────
//  SessionSettings
//  Bound from appsettings.json → "SessionSettings"
//  Used only by HelpdeskTicket.Web.
// ─────────────────────────────────────────────────────────────
public sealed class SessionSettings
{
    public const string SectionName = "SessionSettings";

    public int    IdleTimeoutMinutes { get; init; } = 480;
    public string CookieName        { get; init; } = ".HelpdeskTicket.Session";
    public bool   HttpOnly          { get; init; } = true;

    // "Always" or "SameAsRequest"
    public string SecurePolicy      { get; init; } = "Always";
}
