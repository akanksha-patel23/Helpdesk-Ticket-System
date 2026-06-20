using HelpdeskTicket.Application.Data;
using HelpdeskTicket.Application.DTOs.User;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Constants;
using HelpdeskTicket.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace HelpdeskTicket.Application.Repositories;

// TODO: Implement in step-by-step coding phase
public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbConnectionFactory db, ILogger<UserRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetUserListAsync(int requestedRoleId, int requestedById)
    {
        var list = new List<UserDto>();
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetUserList);
            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedRoleId", requestedRoleId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedById", requestedById));

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new UserDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    FullName = SqlHelper.GetString(reader, "FullName"),
                    Email = SqlHelper.GetString(reader, "Email"),
                    RoleId = SqlHelper.GetInt(reader, "RoleId"),
                    RoleName = SqlHelper.GetString(reader, "RoleName"),
                    IsActive = SqlHelper.GetBool(reader, "IsActive"),
                    //CreatedDateTime = SqlHelper.GetDateTime(reader, "CreatedDateTime")
                    CreatedDateTime = DateTimeHelper.ToIst(SqlHelper.GetDateTime(reader, "CreatedDateTime"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserListAsync failed RoleId:{Role}", requestedRoleId);
            throw;
        }
        return list;
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetUserById);
            cmd.Parameters.Add(SqlHelper.ParamInt("@Id", userId));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    FullName = SqlHelper.GetString(reader, "FullName"),
                    Email = SqlHelper.GetString(reader, "Email"),
                    RoleId = SqlHelper.GetInt(reader, "RoleId"),
                    RoleName = SqlHelper.GetString(reader, "RoleName"),
                    IsActive = SqlHelper.GetBool(reader, "IsActive"),
                    //CreatedDateTime = SqlHelper.GetDateTime(reader, "CreatedDateTime")
                    CreatedDateTime = DateTimeHelper.ToIst(SqlHelper.GetDateTime(reader, "CreatedDateTime"))
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserByIdAsync failed UserId:{Id}", userId);
            throw;
        }
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(UpdateUserRequest request)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.UpdateUser);
            cmd.Parameters.Add(SqlHelper.ParamInt("@Id", request.UserId));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@FullName", request.FullName, 100));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Email", request.Email, 150));
            cmd.Parameters.Add(SqlHelper.ParamTinyInt("@RoleId", (byte)request.RoleId));
            cmd.Parameters.Add(SqlHelper.ParamBit("@IsActive", request.IsActive));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return (SqlHelper.GetInt(reader, "Success") == 1,
                        SqlHelper.GetString(reader, "Message"));

            return (false, "No response from update procedure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateUserAsync failed UserId:{Id}", request.UserId);
            throw;
        }
    }
    public async Task<List<UserDto>> GetDevelopersAsync()
    {
        var list = new List<UserDto>();
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetDeveloperList);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(new UserDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    FullName = SqlHelper.GetString(reader, "FullName")
                });
        }
        catch (Exception ex) { _logger.LogError(ex, "GetDevelopersAsync failed."); throw; }
        return list;
    }
}
