namespace HelpdeskTicket.Web.Helpers;
// ─────────────────────────────────────────────────────────────
//  AuthHeaderHandler  (DelegatingHandler)
//
//  Automatically attaches the JWT token stored in Session to
//  every outgoing HttpClient request made to the API.
//
//  Registered on the "HelpdeskAPI" named HttpClient in Program.cs.
//  Runs before the request leaves the Web application.
//
//  Flow:
//   Web Service calls _http.GetAsync(...)
//     → AuthHeaderHandler.SendAsync()
//         reads token from Session
//         adds "Authorization: Bearer {token}" header
//     → request sent to API
//     → API [Authorize] validates the token
// ─────────────────────────────────────────────────────────────
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var session = _httpContextAccessor.HttpContext?.Session;

        if (session is not null)
        {
            var token = SessionHelper.GetToken(session);

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
