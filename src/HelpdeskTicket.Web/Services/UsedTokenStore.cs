namespace HelpdeskTicket.Web.Services;

// Singleton — lives for app lifetime
// Tracks already-used reset tokens so link can't be reused
public class UsedTokenStore
{
    private readonly HashSet<string> _used = [];
    //private readonly Lock _lock = new();
    private readonly object _lock = new();

    public bool TryUse(string token)
    {
        lock (_lock)
        {
            // Returns true if token is NEW (first use), false if already used
            return _used.Add(token);
        }
    }
}
