using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// 任务执行器服务接口，负责管理查询任务的并发执行和生命周期
/// </summary>
public interface ITaskExecutorService
{
    /// <summary>
    /// 任务完成时触发（成功或失败），用于通知 UI 层
    /// </summary>
    event Action<QueryTask>? TaskCompleted;

    /// <summary>
    /// 提交任务到执行器，立即开始执行（受并发数限制）
    /// </summary>
    /// <param name="task">要执行的查询任务</param>
    Task SubmitAsync(QueryTask task);

    /// <summary>
    /// 取消指定任务
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    void Cancel(Guid taskId);

    /// <summary>
    /// 取消所有正在执行或等待中的任务
    /// </summary>
    void CancelAll();

    /// <summary>
    /// 更新最大并发数（运行时可调整）
    /// </summary>
    /// <param name="maxConcurrency">新的最大并发数</param>
    void UpdateMaxConcurrency(int maxConcurrency);
}
