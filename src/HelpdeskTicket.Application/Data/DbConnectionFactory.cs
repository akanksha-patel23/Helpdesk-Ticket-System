using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HelpdeskTicket.Application.Data;

// ─────────────────────────────────────────────────────────────
//  DbConnectionFactory
//  Responsible for creating and opening SqlConnection instances.
//  Registered as Scoped in DI — one per HTTP request.
//
//  All repositories receive this via constructor injection.
//  No connection pooling code needed — Microsoft.Data.SqlClient
//  manages the connection pool automatically.
// ─────────────────────────────────────────────────────────────
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new, already-opened SqlConnection.
    /// Caller is responsible for disposing (use 'await using').
    /// </summary>
    Task<SqlConnection> CreateConnectionAsync();
}

public sealed class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("HelpdeskDb")
            ?? throw new InvalidOperationException(
                "Connection string 'HelpdeskDb' is missing from appsettings.json.");
    }

    public async Task<SqlConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
