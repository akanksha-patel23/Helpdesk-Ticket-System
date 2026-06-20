using HelpdeskTicket.Application.Data;
using HelpdeskTicket.Application.DTOs.Category;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Constants;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(IDbConnectionFactory db, ILogger<CategoryRepository> logger)
    {
        _db = db;
        _logger = logger;
    }
    // includeInactive = false → active only (ticket form dropdowns)
    // includeInactive = true  → all categories (admin management page)
    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive = false)
    {
        var list = new List<CategoryDto>();
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetAllCategory);
            cmd.Parameters.Add(SqlHelper.ParamBit("@IncludeInactive", includeInactive));

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                list.Add(new CategoryDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    Name = SqlHelper.GetString(reader, "Name"),
                    IsActive = SqlHelper.GetBool(reader, "IsActive")
                });
        }
        catch (Exception ex) { _logger.LogError(ex, "GetAllAsync categories failed."); throw; }
        return list;
    }

    public async Task<(bool Success, string Message)> SaveAsync(SaveCategoryRequest request)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.SaveCategory);
            cmd.Parameters.Add(SqlHelper.ParamInt("@Id", request.Id));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Name", request.Name, 100));
            cmd.Parameters.Add(SqlHelper.ParamBit("@IsActive", request.IsActive));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var id = SqlHelper.GetInt(reader, "Id");
                var message = SqlHelper.GetString(reader, "Message");
                return (id > 0, message);
            }
            return (false, "No response from save procedure.");
        }
        catch (Exception ex) { _logger.LogError(ex, "SaveAsync category failed."); throw; }
    }
    // Soft delete — sets IsActive = false via spSaveCategory
    //public async Task<(bool Success, string Message)> DeleteAsync(int id)
    //{
    //    try
    //    {
    //        var all = await GetAllAsync(includeInactive: true);
    //        var current = all.FirstOrDefault(c => c.Id == id);

    //        if (current is null)
    //            return (false, "Category not found.");

    //        return await SaveAsync(new SaveCategoryRequest
    //        {
    //            Id = id,
    //            Name = current.Name,
    //            IsActive = false
    //        });
    //    }
    //    catch (Exception ex) { _logger.LogError(ex, "DeleteAsync category failed Id:{Id}", id); throw; }
    //}
}
