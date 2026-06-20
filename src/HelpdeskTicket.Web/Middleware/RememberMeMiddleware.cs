using HelpdeskTicket.Web.Helpers;
using System.IdentityModel.Tokens.Jwt;

namespace HelpdeskTicket.Web.Middleware;    

public class RememberMeMiddleware
{
    private readonly RequestDelegate _next;

    public RememberMeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only run if session has no token but persistent cookie exists
        var session = context.Session;
        var token = SessionHelper.GetToken(session);

        if (string.IsNullOrEmpty(token) &&
            context.Request.Cookies.TryGetValue("hd_remember", out var rememberedToken) &&
            !string.IsNullOrWhiteSpace(rememberedToken))
        {
            try
            {
                // Decode JWT claims without validation — just to restore session fields
                // Token signature is validated by the API on every call anyway
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(rememberedToken);

                var userId = int.Parse(jwt.Claims.First(c => c.Type == "userId").Value);
                var roleId = int.Parse(jwt.Claims.First(c => c.Type == "roleId").Value);
                var email = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
                var fullName = jwt.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;
                var roleName = jwt.Claims.First(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Value;

                // Token expired — clear cookie and let user log in again
                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    context.Response.Cookies.Delete("hd_remember");
                    await _next(context);
                    return;
                }

                // Restore session from cookie
                SessionHelper.SetUserSession(session, userId, fullName, email,
                    roleId, roleName, rememberedToken);
            }
            catch
            {
                // Invalid token — clear cookie silently
                context.Response.Cookies.Delete("hd_remember");
            }
        }

        await _next(context);
    }
}
