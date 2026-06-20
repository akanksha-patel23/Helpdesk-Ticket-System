using HelpdeskTicket.Application.Data;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Application.Models;
using HelpdeskTicket.Core.Constants;
using Microsoft.Extensions.Logging;

namespace HelpdeskTicket.Application.Repositories;

// TODO: Implement in step-by-step coding phase
public class AuthRepository : IAuthRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(IDbConnectionFactory db, ILogger<AuthRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetUserByEmail);
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Email", email, 150));

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    FullName = SqlHelper.GetString(reader, "FullName"),
                    Email = SqlHelper.GetString(reader, "Email"),
                    PasswordHash = SqlHelper.GetString(reader, "PasswordHash"),
                    RoleId = SqlHelper.GetByte(reader, "RoleId"),
                    RoleName = SqlHelper.GetString(reader, "RoleName"),
                    IsActive = SqlHelper.GetBool(reader, "IsActive")
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserByEmailAsync failed for {Email}", email);
            throw;
        }
    }

    public async Task<int> RegisterUserAsync(string fullName, string email, string passwordHash, int roleId)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.RegisterUser);
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@FullName", fullName, 100));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Email", email, 150));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@PasswordHash", passwordHash, -1));  // nvarchar(max)
            cmd.Parameters.Add(SqlHelper.ParamTinyInt("@RoleId", (byte)roleId));

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
                return SqlHelper.GetInt(reader, "Id");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegisterUserAsync failed for {Email}", email);
            throw;
        }
    }
    public async Task<bool> UpdatePasswordAsync(string email, string passwordHash)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.UpdatePassword);
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Email", email, 150));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@PasswordHash", passwordHash, -1));

            //var rows = await cmd.ExecuteNonQueryAsync();
            //return rows > 0;
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var rows = Convert.ToInt32(reader["RowsAffected"]);
                _logger.LogInformation("RowsAffected from SP = {Rows}", rows, email);
                return rows > 0;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdatePasswordAsync failed for {Email}", email);
            throw;
        }
    }
    public async Task<bool> SavePasswordResetTokenAsync(string email, Guid token, DateTime expiry)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.SavePasswordResetToken);
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Email", email, 150));
            cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Token", token));
            cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Expiry", expiry));

            await using var reader = await cmd.ExecuteReaderAsync();
            //if (await reader.ReadAsync())
            //    return SqlHelper.GetInt(reader, "RowsAffected") > 0;
            //return false;
            if (await reader.ReadAsync())
            {
                var rows = SqlHelper.GetInt(reader, "RowsAffected");
                _logger.LogInformation("SavePasswordResetTokenAsync: RowsAffected={Rows} Email={Email} Token={Token} Expiry={Expiry}",
                    rows, email, token, expiry);
                return rows > 0;
            }

            _logger.LogWarning("SavePasswordResetTokenAsync: No result row returned from SP for Email={Email}", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SavePasswordResetTokenAsync failed for {Email}", email);
            throw;
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordByTokenAsync(
        Guid token, string passwordHash)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.ResetPasswordByToken);
            cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Token", token));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@PasswordHash", passwordHash, -1));

            await using var reader = await cmd.ExecuteReaderAsync();
            //if (await reader.ReadAsync())
            //    return (SqlHelper.GetInt(reader, "Success") == 1,
            //            SqlHelper.GetString(reader, "Message"));

            //return (false, "No response from reset procedure.");
            if (await reader.ReadAsync())
            {
                var success = SqlHelper.GetInt(reader, "Success") == 1;
                var message = SqlHelper.GetString(reader, "Message");
                _logger.LogInformation("ResetPasswordByTokenAsync: SP returned Success={Success} Message={Message} Token={Token}",
                    success, message, token);
                return (success, message);
            }

            _logger.LogWarning("ResetPasswordByTokenAsync: No response row from SP for Token={Token}", token);
            return (false, "No response from reset procedure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ResetPasswordByTokenAsync failed");
            throw;
        }
    }
    public async Task<bool> ValidateResetTokenAsync(Guid token)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.ValidatePasswordResetToken);
            cmd.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Token", token));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return SqlHelper.GetInt(reader, "IsValid") == 1;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateResetTokenAsync failed for Token:{Token}", token);
            return false;
        }
    }
}
