using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// 数据库执行服务接口，负责连接数据库并执行 SQL 查询
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// 执行 SQL 查询，返回所有结果集
    /// 支持多结果集（一条 SQL 包含多个 SELECT）
    /// </summary>
    /// <param name="config">目标数据库配置</param>
    /// <param name="sql">SQL 语句</param>
    /// <param name="parameters">参数化查询的参数列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>查询结果（包含所有结果集）</returns>
    Task<QueryResult> ExecuteQueryAsync(
        DatabaseConfig config,
        string sql,
        List<QueryParameter> parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 测试数据库连接是否正常
    /// </summary>
    /// <param name="config">要测试的数据库配置</param>
    /// <returns>连接成功返回 true，失败返回 false</returns>
    Task<(bool Success, string Message)> TestConnectionAsync(DatabaseConfig config);
}
