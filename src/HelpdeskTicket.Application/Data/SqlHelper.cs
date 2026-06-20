using System.Data;
using Microsoft.Data.SqlClient;

namespace HelpdeskTicket.Application.Data;

// ─────────────────────────────────────────────────────────────
//  SqlHelper
//  Reusable ADO.NET utilities for Stored Procedure execution.
//  Keeps repository code clean — no repeated boilerplate.
//
//  Design rules:
//  • Every method works with CommandType.StoredProcedure only
//  • Parameters are always passed; never string-concatenated
//  • Callers pass an open SqlConnection (from DbConnectionFactory)
//  • All methods are async
// ─────────────────────────────────────────────────────────────
public static class SqlHelper
{
    // ── Create a stored-procedure command ────────────────────

    /// <summary>
    /// Creates a SqlCommand configured for stored procedure execution.
    /// </summary>
    public static SqlCommand CreateCommand(
        SqlConnection connection,
        string storedProcedureName,
        SqlTransaction? transaction = null)
    {
        var cmd = new SqlCommand(storedProcedureName, connection)
        {
            CommandType    = CommandType.StoredProcedure,
            CommandTimeout = 30
        };

        if (transaction is not null)
            cmd.Transaction = transaction;

        return cmd;
    }

    // ── Parameter factory helpers ────────────────────────────

    public static SqlParameter Param(string name, object? value)
        => new(name, value ?? DBNull.Value);

    public static SqlParameter ParamNVarChar(string name, string? value, int size)
        => new(name, SqlDbType.NVarChar, size) { Value = (object?)value ?? DBNull.Value };

    public static SqlParameter ParamInt(string name, int? value)
        => new(name, SqlDbType.Int) { Value = (object?)value ?? DBNull.Value };

    public static SqlParameter ParamTinyInt(string name, byte? value)
        => new(name, SqlDbType.TinyInt) { Value = (object?)value ?? DBNull.Value };

    public static SqlParameter ParamBit(string name, bool value)
        => new(name, SqlDbType.Bit) { Value = value };

    // ── Safe reader helpers (handle DBNull) ──────────────────

    public static int GetInt(SqlDataReader reader, string column)
        => reader[column] == DBNull.Value ? 0 : Convert.ToInt32(reader[column]);

    public static int? GetNullableInt(SqlDataReader reader, string column)
        => reader[column] == DBNull.Value ? null : Convert.ToInt32(reader[column]);

    public static string GetString(SqlDataReader reader, string column)
        => reader[column] == DBNull.Value ? string.Empty : reader[column].ToString()!;

    public static string? GetNullableString(SqlDataReader reader, string column)
        => reader[column] == DBNull.Value ? null : reader[column].ToString();

    public static bool GetBool(SqlDataReader reader, string column)
        => reader[column] != DBNull.Value && Convert.ToBoolean(reader[column]);

    //public static DateTime GetDateTime(SqlDataReader reader, string column)
    //    => reader[column] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader[column]);

    //public static DateTime? GetNullableDateTime(SqlDataReader reader, string column)
    //    => reader[column] == DBNull.Value ? null : Convert.ToDateTime(reader[column]);
    public static DateTime GetDateTime(SqlDataReader reader, string column)
    => reader.GetDateTime(reader.GetOrdinal(column));

    public static DateTime? GetNullableDateTime(SqlDataReader reader, string column)
        => reader[column] == DBNull.Value
            ? null
            : reader.GetDateTime(reader.GetOrdinal(column));

    public static byte GetByte(SqlDataReader reader, string column)
        => reader[column] == DBNull.Value ? (byte)0 : Convert.ToByte(reader[column]);
}
