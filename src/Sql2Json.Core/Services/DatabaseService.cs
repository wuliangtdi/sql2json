using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using Sql2Json.Core.Enums;
using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// 数据库执行服务实现
/// 根据数据库类型动态创建连接，执行参数化 SQL 查询，支持多结果集
/// </summary>
public class DatabaseService : IDatabaseService
{
    /// <inheritdoc/>
    public async Task<QueryResult> ExecuteQueryAsync(
        DatabaseConfig config,
        string sql,
        List<QueryParameter> parameters,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new QueryResult();

        // 根据数据库类型创建对应的连接
        await using var connection = CreateConnection(config);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 300; // 5 分钟超时

        // 绑定参数化查询的参数
        foreach (var param in parameters)
        {
            var dbParam = command.CreateParameter();
            dbParam.ParameterName = param.Name;
            dbParam.Value = string.IsNullOrEmpty(param.Value) ? DBNull.Value : param.Value;
            command.Parameters.Add(dbParam);
        }

        // 执行查询并读取所有结果集
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var resultSetIndex = 0;

        do
        {
            var resultSet = new ResultSet { Index = resultSetIndex };

            // 读取列信息
            for (var i = 0; i < reader.FieldCount; i++)
            {
                resultSet.Columns.Add(reader.GetName(i));
            }

            // 读取所有行数据
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value;
                }
                resultSet.Rows.Add(row);
            }

            result.ResultSets.Add(resultSet);
            resultSetIndex++;
        }
        // NextResult() 移动到下一个结果集（多 SELECT 语句时）
        while (await reader.NextResultAsync(cancellationToken));

        stopwatch.Stop();
        result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        return result;
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string Message)> TestConnectionAsync(DatabaseConfig config)
    {
        try
        {
            await using var connection = CreateConnection(config);
            await connection.OpenAsync();
            await connection.CloseAsync();
            return (true, "连接成功");
        }
        catch (Exception ex)
        {
            return (false, $"连接失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 根据数据库类型创建对应的 DbConnection 实例
    /// </summary>
    /// <param name="config">数据库配置</param>
    /// <returns>对应类型的数据库连接</returns>
    /// <exception cref="NotSupportedException">不支持的数据库类型</exception>
    private static DbConnection CreateConnection(DatabaseConfig config)
    {
        return config.DatabaseType switch
        {
            DatabaseType.SqlServer => new SqlConnection(config.ConnectionString),
            DatabaseType.MySql => new MySqlConnection(config.ConnectionString),
            DatabaseType.MariaDb => new MySqlConnection(config.ConnectionString),
            DatabaseType.PostgreSql => new NpgsqlConnection(config.ConnectionString),
            DatabaseType.Oracle => new OracleConnection(config.ConnectionString),
            DatabaseType.Sqlite => new SqliteConnection(config.ConnectionString),
            _ => throw new NotSupportedException($"不支持的数据库类型: {config.DatabaseType}")
        };
    }
}
