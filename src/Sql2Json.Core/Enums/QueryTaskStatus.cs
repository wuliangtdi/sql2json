namespace Sql2Json.Core.Enums;

/// <summary>
/// 查询任务的执行状态
/// </summary>
public enum QueryTaskStatus
{
    /// <summary>
    /// 等待执行
    /// </summary>
    Pending,

    /// <summary>
    /// 正在执行中
    /// </summary>
    Running,

    /// <summary>
    /// 执行完成
    /// </summary>
    Completed,

    /// <summary>
    /// 执行失败
    /// </summary>
    Failed,

    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled
}
