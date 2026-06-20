using HelpdeskTicket.Core.Constants;

namespace HelpdeskTicket.Web.Helpers;

// TODO: Implement in step-by-step coding phase
public static class SessionHelper {
    public static void SetUserSession(ISession session, int userId, string fullName,
    string email, int roleId, string roleName, string token)
    {
        session.SetString(SessionKeys.JwtToken, token);
        session.SetInt32(SessionKeys.UserId, userId);
        session.SetString(SessionKeys.FullName, fullName);
        session.SetString(SessionKeys.Email, email);
        session.SetInt32(SessionKeys.RoleId, roleId);
        session.SetString(SessionKeys.RoleName, roleName);
    }

    public static void ClearSession(ISession session) => session.Clear();
    public static string? GetToken(ISession session) => session.GetString(SessionKeys.JwtToken);
    public static int GetUserId(ISession session) => session.GetInt32(SessionKeys.UserId) ?? 0;
    public static string GetFullName(ISession session) => session.GetString(SessionKeys.FullName) ?? string.Empty;
    public static string GetEmail(ISession session) => session.GetString(SessionKeys.Email) ?? string.Empty;
    public static int GetRoleId(ISession session) => session.GetInt32(SessionKeys.RoleId) ?? 0;
    public static string GetRoleName(ISession session) => session.GetString(SessionKeys.RoleName) ?? string.Empty;
    public static bool IsLoggedIn(ISession session) => !string.IsNullOrEmpty(GetToken(session));
}
