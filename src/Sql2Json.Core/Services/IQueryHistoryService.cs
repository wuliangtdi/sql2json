using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// 查询历史服务接口，负责管理常用 SQL 的保存和检索
/// </summary>
public interface IQueryHistoryService
{
    /// <summary>
    /// 获取所有查询历史，按最后使用时间降序排列
    /// </summary>
    Task<List<QueryHistory>> GetAllAsync();

    /// <summary>
    /// 根据关键词搜索查询历史（匹配名称或 SQL 内容）
    /// </summary>
    /// <param name="keyword">搜索关键词</param>
    Task<List<QueryHistory>> SearchAsync(string keyword);

    /// <summary>
    /// 保存查询到历史记录
    /// </summary>
    /// <param name="history">查询历史对象</param>
    Task<QueryHistory> SaveAsync(QueryHistory history);

    /// <summary>
    /// 更新查询历史的使用信息（使用次数+1，更新最后使用时间）
    /// </summary>
    /// <param name="id">历史记录 ID</param>
    Task MarkUsedAsync(Guid id);

    /// <summary>
    /// 删除查询历史
    /// </summary>
    /// <param name="id">历史记录 ID</param>
    Task DeleteAsync(Guid id);
}
